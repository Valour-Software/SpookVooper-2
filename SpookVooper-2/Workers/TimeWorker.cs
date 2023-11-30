namespace SV2.Workers;

public class TimeWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public readonly ILogger<TimeWorker> _logger;

    public TimeWorker(ILogger<TimeWorker> logger,
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
                        DBCache.CurrentTime.Time = DBCache.CurrentTime.Time.AddSeconds(3);

                        //await Task.Delay(1000 * 60 * 60);
                        await Task.Delay(1000);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("FATAL TIME WORKER ERROR:");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        if (e.InnerException is not null)
                            Console.WriteLine(e.InnerException);
                    }
                }
            });

            while (!task.IsCompleted)
            {
                await Task.Delay(30000);
            }
        }
    }
}