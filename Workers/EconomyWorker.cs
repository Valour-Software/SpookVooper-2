using SV2.Database;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using SV2.Web;

namespace SV2.Workers
{
    public class EconomyWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public readonly ILogger<EconomyWorker> _logger;

        public EconomyWorker(ILogger<EconomyWorker> logger,
                            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

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
                            List<GroupRole>? roles = DBCache.GetAll<GroupRole>().ToList();

                            foreach(GroupRole role in roles) {
                                if (role.Salary > 0.1m) {
                                    TaxCreditPolicy taxcredit = DBCache.GetAll<TaxCreditPolicy>().FirstOrDefault(x => x.DistrictId == role.Group.DistrictId && x.taxCreditType == TaxCreditType.Employee);
                                    foreach(long Id in role.Members) {
                                        Transaction tran = new Transaction(role.GroupId, Id, role.Salary, TransactionType.Paycheck, $"{role.Name} Salary");
                                        TaskResult result = await tran.Execute();
                                        if (!result.Succeeded) {
                                            // no sense to keep paying these members since the group has ran out of credits
                                            break;
                                        }
                                        if (taxcredit is not null) {
                                            Transaction TaxCreditTran = new Transaction(taxcredit.DistrictId!, role.GroupId, role.Salary*taxcredit.Rate, TransactionType.TaxCreditPayment, $"Employee Tax Credit Payment");
                                            TaxCreditTran.NonAsyncExecute();
                                        }
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
                                if (policy.DistrictId != null) {
                                    effected = effected.Where(x => x.DistrictId == policy.DistrictId).ToList();
                                    fromId = policy.DistrictId;
                                }
                                if (policy.ApplicableRank != null) {
                                    effected = effected.Where(x => x.Rank == policy.ApplicableRank).ToList();
                                }
                                foreach(SVUser user in effected) {
                                    decimal rate = policy.Rate;

                                    // if the user has joined less than 4 weeks ago
                                    if (DateTime.UtcNow.Subtract(user.Joined).Days <= 28) {
                                        decimal increase = 2.0m;
                                        if (DateTime.UtcNow.Subtract(user.Joined).Days >= 7) {
                                            increase -= Math.Min(0, DateTime.UtcNow.Subtract(user.Joined).Days-7)/21*2;
                                        }
                                        rate *= increase+1;
                                    }
                                    Transaction tran = new Transaction(fromId, user.Id, rate/24.0m, TransactionType.Paycheck, $"UBI for rank {policy.ApplicableRank.ToString()}");
                                    tran.NonAsyncExecute();
                                }
                            }

                            if (DateTime.UtcNow.Hour == 1) {
                                // every day, update credit snapchats
                                List<BaseEntity> entities = DBCache.GetAll<SVUser>().ToList<BaseEntity>();
                                entities.AddRange(DBCache.GetAll<Group>());
                                foreach(BaseEntity entity in entities)
                                {
                                    if (entity.CreditSnapshots is null) {
                                        entity.CreditSnapshots = new();
                                    }
                                    entity.CreditSnapshots.Add(entity.TaxAbleBalance);
                                    await entity.DoIncomeTax();
                                }
                            }

                            await Task.Delay(1000 * 60 * 60);
                        }
                        catch(System.Exception e)
                        {
                            Console.WriteLine("FATAL ECONOMY WORKER ERROR:");
                            Console.WriteLine(e.Message);
                        }
                    }
                });

                while (!task.IsCompleted)
                {
                    _logger.LogInformation("Economy Worker running at: {time}", DateTimeOffset.Now);
                    // for right now, just save cache to database every 2 minutes
                    await DBCache.SaveAsync();
                    await Task.Delay(120_000, stoppingToken);
                }

                _logger.LogInformation("Economy Worker task stopped at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Restarting.", DateTimeOffset.Now);
            }
        }
    }
}