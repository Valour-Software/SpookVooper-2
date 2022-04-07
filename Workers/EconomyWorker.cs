using SV2.Database;
using SV2.Database.Models.Groups;
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
                                    foreach(String Id in role.Members) {
                                        Transaction tran = new()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Credits = role.Salary,
                                            Time = DateTime.UtcNow,
                                            FromId = role.GroupId,
                                            ToId = Id,
                                            transactionType = TransactionType.Paycheck,
                                            Details = $"{role.Name} Salary"
                                        };
                                        TaskResult result = await tran.Execute();
                                        if (!result.Succeeded) {
                                            // no sense to keep paying these members since the group has ran out of credits
                                            break;
                                        }
                                    }
                                }
                                    
                            }

                            List<UBIPolicy>? UBIPolicies = DBCache.GetAll<UBIPolicy>().ToList();

                            foreach(UBIPolicy policy in UBIPolicies) {
                                List<User> effected = DBCache.GetAll<User>().ToList();
                                string fromId = "";
                                if (policy.DistrictId != null) {
                                    effected = effected.Where(x => x.DistrictId == policy.DistrictId).ToList();
                                    fromId = policy.DistrictId;
                                }
                                if (policy.ApplicableRank != null) {
                                    effected = effected.Where(x => x.Rank == policy.ApplicableRank).ToList();
                                    fromId = "g-vooperia";
                                }
                                foreach(User user in effected) {
                                    Transaction tran = new()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Credits = policy.Rate,
                                        Time = DateTime.UtcNow,
                                        FromId = fromId,
                                        ToId = user.Id,
                                        transactionType = TransactionType.Paycheck,
                                        Details = $"UBI for rank {policy.ApplicableRank.ToString()}"
                                    };
                                    TaskResult result = await tran.Execute();
                                    if (!result.Succeeded) {
                                        // no sense to keep paying these members since the group has ran out of credits
                                        break;
                                    }
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
                    await Task.Delay(60000, stoppingToken);
                }

                _logger.LogInformation("Economy Worker task stopped at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Restarting.", DateTimeOffset.Now);
            }
        }
    }
}