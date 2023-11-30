namespace SV2.Workers;

public class VoopAIWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public readonly ILogger<VoopAIWorker> _logger;

    public VoopAIWorker(ILogger<VoopAIWorker> logger,
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
                        await VoopAI.VoopAI.UpdateRanks();

                        await Task.Delay(1000 * 60 * 5);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("FATAL VOOPAI WORKER ERROR:");
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