using SV2.Database;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using SV2.Web;

namespace SV2.Workers
{
    public class TransactionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public readonly ILogger<EconomyWorker> _logger;
        private static VooperDB dbctx;
        private static DateTime LastTime = DateTime.UtcNow;

        public TransactionWorker(ILogger<EconomyWorker> logger,
                            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            dbctx = VooperDB.DbFactory.CreateDbContext();
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
                            if (!(await TransactionManager.Run(dbctx)))
                            {
                                await Task.Delay(10);
                            }
                        }
                        catch(System.Exception e)
                        {
                            Console.WriteLine("FATAL TRANSACTION WORKER ERROR:");
                            Console.WriteLine(e.Message);
                        }
                        if (TransactionManager.transactionQueue.IsEmpty || (DateTime.UtcNow - LastTime).TotalMinutes >= 1)
                        {
                            await dbctx.SaveChangesAsync();
                            LastTime = DateTime.UtcNow;
                        }
                    }
                });

                while (!task.IsCompleted)
                {
                    _logger.LogInformation("TRANSACTION Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(60000);
                }

                _logger.LogInformation("TRANSACTION Worker task stopped at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Restarting.", DateTimeOffset.Now);
            }
        }
    }
}