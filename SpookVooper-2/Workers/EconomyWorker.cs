using IdGen;
using SV2.Database;
using SV2.Database.Managers;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using SV2.Web;
using System.Data;
using System.Diagnostics;

namespace SV2.Workers
{
    public class EconomyWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public readonly ILogger<EconomyWorker> _logger;

        private readonly VooperDB _dbctx;

        public EconomyWorker(ILogger<EconomyWorker> logger,
                            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _dbctx = VooperDB.DbFactory.CreateDbContext();
        }

        // TODO: optimize this to heck
        // it is very unoptimized because I am just trying to get this shipped
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Task task = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            // do district funding
                            foreach(var district in DBCache.GetAll<District>())
                            {
                                decimal amount = (decimal)Defines.NDistrict[NDistrict.DISTRICT_FUNDING_BASE];
                                amount += (decimal)((double)district.Citizens.Count * Defines.NDistrict[NDistrict.DISTRICT_FUNDING_PER_CITIZEN]);
                                var tran = new SVTransaction(BaseEntity.Find(100), BaseEntity.Find(district.GroupId), amount/30/24, TransactionType.FreeMoney, $"Imperial District Funding for {district.Name}");
                                TaskResult result = await tran.Execute();
                            }
                            List<GroupRole>? roles = DBCache.GetAll<GroupRole>().ToList();

                            foreach(GroupRole role in roles) {
                                if (role.Salary > 0.1m) {
                                    TaxCreditPolicy taxcredit = DBCache.GetAll<TaxCreditPolicy>().FirstOrDefault(x => x.DistrictId == role.Group.DistrictId && x.taxCreditType == TaxCreditType.Employee);
                                    decimal amount = 0.00m;
                                    foreach(long Id in role.MembersIds) {
                                        var tran = new SVTransaction(BaseEntity.Find(role.GroupId), BaseEntity.Find(Id), role.Salary, TransactionType.Paycheck, $"{role.Name} Salary");
                                        TaskResult result = await tran.Execute();
                                        if (!result.Succeeded) {
                                            // no sense to keep paying these members since the group has ran out of credits
                                            break;
                                        }
                                        if (taxcredit is not null) {
                                            amount += role.Salary;
                                        }
                                    }
                                    if (taxcredit is not null && amount > 0.00m)
                                    {
                                        var TaxCreditTran = new SVTransaction(BaseEntity.Find(taxcredit.DistrictId!), BaseEntity.Find(role.GroupId), amount * taxcredit.Rate, TransactionType.TaxCreditPayment, $"Employee Tax Credit Payment");
                                        TaxCreditTran.NonAsyncExecute();
                                    }
                                }  
                            }

                            List<UBIPolicy>? UBIPolicies = DBCache.GetAll<UBIPolicy>().ToList();

                            foreach(UBIPolicy policy in UBIPolicies) {
                                if (policy.Rate <= 0.0m)
                                {
                                    continue;
                                }
                                List<SVUser> effected = DBCache.GetAll<SVUser>().ToList();
                                long fromId = 100;
                                if (policy.DistrictId != 100) {
                                    effected = effected.Where(x => x.DistrictId == policy.DistrictId).ToList();
                                    fromId = policy.DistrictId;
                                }
                                if (policy.ApplicableRank is not null) {
                                    effected = effected.Where(x => x.Rank == policy.ApplicableRank).ToList();
                                }
                                foreach(SVUser user in effected) {
                                    decimal rate = policy.Rate;

                                    // if the user has joined less than 6 weeks ago
                                    if (DateTime.UtcNow.Subtract(user.Joined).Days <= 42) {
                                        decimal increase = 3.0m;
                                        if (DateTime.UtcNow.Subtract(user.Joined).Days >= 7) {
                                            increase -= Math.Min(0, DateTime.UtcNow.Subtract(user.Joined).Days-7)/35*3;
                                        }
                                        rate *= increase+1;
                                    }
                                    rate = policy.Rate * 5.0m;
                                    var tran = new SVTransaction(BaseEntity.Find(fromId), BaseEntity.Find(user.Id), rate/24.0m, TransactionType.Paycheck, $"UBI for rank {policy.ApplicableRank.ToString()}");
                                    tran.NonAsyncExecute();
                                }
                            }

                            if (DateTime.UtcNow.Hour == 1) {
                                // every day, update credit snapchats
                                List<BaseEntity> entities = DBCache.GetAll<SVUser>().ToList<BaseEntity>();
                                entities.AddRange(DBCache.GetAll<Group>());
                                List<EntityBalanceRecord> records = new();
                                foreach(BaseEntity entity in entities)
                                {
                                    records.Add(new() {
                                        EntityId = entity.Id,
                                        Time = DateTime.UtcNow,
                                        Balance = await entity.GetCreditsAsync(),
                                        TaxableBalance = entity.TaxAbleBalance
                                    });
                                }
                                _dbctx.AddRange(records);
                                await _dbctx.SaveChangesAsync();

                                foreach(BaseEntity entity in entities) {
                                    await entity.DoIncomeTax(_dbctx);
                                }
                            }

                            Stopwatch sw = Stopwatch.StartNew();
                            
                            // entityid : dict<governorid, amount to pay>
                            Dictionary<long, Dictionary<long, double>> propertytaxes = new();
                            List<ProducingBuilding> buildings = DBCache.GetAll<Factory>().Select(x => (ProducingBuilding)x).ToList();
                            buildings.AddRange(DBCache.GetAll<Mine>().Select(x => (ProducingBuilding)x).ToList());

                            foreach (var building in buildings)
                            {
                                if (building.BuildingType == BuildingType.Infrastructure) continue;
                                if (!propertytaxes.ContainsKey(building.OwnerId))
                                    propertytaxes[building.OwnerId] = new();
                                var entitytaxes = propertytaxes[building.OwnerId];
                                var id = building.Province.GovernorId ?? building.DistrictId;
                                if (!entitytaxes.ContainsKey(id))
                                    entitytaxes[id] = 0.00;
                                double amount = building.Province.BasePropertyTax ?? 0;
                                amount += (building.Province.PropertyTaxPerSize ?? 0) * building.Size;
                                entitytaxes[id] += amount;

                                // now we do state property taxes
                                if (building.Province.StateId is not null)
                                {
                                    var stateid = (long)building.Province.StateId;
                                    if (!entitytaxes.ContainsKey(stateid))
                                        entitytaxes[stateid] = 0.00;

                                    amount = building.Province.State!.BasePropertyTax ?? 0;
                                    amount += (building.Province.State!.PropertyTaxPerSize ?? 0) * building.Size;
                                    entitytaxes[stateid] += amount;
                                }

                                // now we do district property taxes
                                if (!entitytaxes.ContainsKey(building.DistrictId))
                                    entitytaxes[building.DistrictId] = 0.00;
                                
                                amount = building.District.BasePropertyTax ?? 0;
                                amount += (building.District.PropertyTaxPerSize ?? 0) * building.Size;
                                entitytaxes[building.DistrictId] += amount;
                            }
                            sw.Stop();
                            Console.WriteLine($"Time took to determine property taxes: {(int)(sw.Elapsed.TotalMilliseconds)}ms");

                            foreach((var entityid, var paymentpergovernorid) in propertytaxes) 
                            {
                                foreach (var governorid in paymentpergovernorid.Keys)
                                {
                                    if (paymentpergovernorid[governorid] <= 0) break;
                                    var tran = new SVTransaction(BaseEntity.Find(entityid), BaseEntity.Find(governorid), (decimal)(paymentpergovernorid[governorid]/30/24), TransactionType.TaxPayment, $"Property Tax Payment");
                                    tran.NonAsyncExecute();
                                }
                            }

                            //await Task.Delay(1000 * 60 * 60);
                            await Task.Delay(1000 * 60 * 5);
                        }
                        catch(System.Exception e)
                        {
                            Console.WriteLine("FATAL ECONOMY WORKER ERROR:");
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                            if (e.InnerException is not null)
                                Console.WriteLine(e.InnerException);
                        }
                    }
                });

                while (!task.IsCompleted)
                {
                    _logger.LogInformation("Economy Worker running at: {time}", DateTimeOffset.Now);
                    // for right now, just save cache to database every 2 minutes
                    await DBCache.SaveAsync();
#if DEBUG
                    await Task.Delay(10_000, stoppingToken);
#else
                    await Task.Delay(30_000, stoppingToken);
#endif
                }

                _logger.LogInformation("Economy Worker task stopped at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Restarting.", DateTimeOffset.Now);
            }
        }
    }
}