using SV2.Database;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using SV2.Web;
using System.Diagnostics;

namespace SV2.Workers;

public class DistrictUpdateWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public readonly ILogger<DistrictUpdateWorker> _logger;
    private static VooperDB dbctx;
    private static DateTime LastTime = DateTime.UtcNow;

    public DistrictUpdateWorker(ILogger<DistrictUpdateWorker> logger,
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
                    int times = 0;
                    try
                    {
                        foreach(var district in DBCache.GetAll<District>())
                            district.ProvincesByDevelopmnet = district.Provinces.OrderByDescending(x => x.DevelopmentValue).ToList();
                        Stopwatch sw = Stopwatch.StartNew();
                        for (int i = 0; i < 1; i++)
                        {
                            foreach (var province in DBCache.GetAll<Province>())
                            {
                                await province.HourlyTick();
                            }
                        }
                        sw.Stop();
                        Console.WriteLine($"Time took to tick provinces: {(int)(sw.Elapsed.TotalMilliseconds)}ms");

                        sw = Stopwatch.StartNew();
                        foreach(var district in DBCache.GetAll<District>())
                        {
                            district.HourlyTick();
                        }
                        sw.Stop();
                        Console.WriteLine($"Time took to tick districts: {(int)(sw.Elapsed.TotalMilliseconds)}ms");
                        if (times%168 == 0)
                            Console.WriteLine(times);
                        //await Task.Delay(1000 * 60 * 60);
                        await Task.Delay(1000 * 60 * 5);
                    }
                    catch(System.Exception e)
                    {
                        Console.WriteLine("FATAL DISTRICT UPDATING WORKER ERROR:");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        if (e.InnerException is not null) {
                            Console.WriteLine(e.InnerException);
                            Console.WriteLine(e.InnerException.StackTrace);
                        }
                    }
                }
            });

            while (!task.IsCompleted)
            {
                _logger.LogInformation("District Updating Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(60000);
            }

            _logger.LogInformation("District Updating Worker task stopped at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("Restarting.", DateTimeOffset.Now);
        }
    }
}