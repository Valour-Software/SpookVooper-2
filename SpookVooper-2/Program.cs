global using SV2.Database;
global using SV2.Database.Models.Districts;
global using SV2.Database.Models.Economy;
global using SV2.Database.Models.Entities;
global using SV2.Database.Models.Factories;
global using SV2.Database.Models.Government;
global using SV2.Database.Models.Groups;
global using SV2.Database.Models.Items;
global using SV2.Database.Models.Military;
global using SV2.Database.Models.Permissions;
global using SV2.Database.Models.Buildings;
global using SV2.Database.Models.Users;
global using SV2.Database.Models.OAuth2;
global using SV2.Models.Districts;
global using SV2.Managers;
global using SV2.Database.Models.Districts.Modifiers;
global using System.Net.Http.Json;
global using Valour.Net.Client;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SV2.API;
using SV2.Workers;
using SV2.Managers;
using SV2.VoopAI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Builder;
using SV2.Database.Managers;
using System.Net;
using SV2.Helpers;
using SV2.Scripting.Parser;

//LuaParser parser = new();

//parser.LoadTokenizer();
//parser.Parse(File.ReadAllText("Managers/Data/BuildingUpgrades/factoryupgrades.lua"), "factoryupgrades.lua");

//string jsonString = JsonSerializer.Serialize((LuaTable)parser.Objects.Items.First(), options: new() { WriteIndented = true});
//await File.WriteAllTextAsync("Managers/Data/ParserOutput.txt", jsonString);

Defines.Load();


var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

builder.Configuration.GetSection("Valour").Get<ValourConfig>();
builder.Configuration.GetSection("Database").Get<DBConfig>();


builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
#if DEBUG
    options.Listen(IPAddress.Any, 7186, listenOptions => {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
#else
    options.Listen(IPAddress.Any, 5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
    });
#endif
});

if (false)
{
    string CONF_LOC = "SV2Config/";
    string DBCONF_FILE = "DBConfig.json";

    // Add services to the container.
    builder.Services.AddMvc(options =>
    {
        options.Filters.Add<UserRequiredAttribute>();
    }
    ).AddRazorRuntimeCompilation();

    // Create directory if it doesn't exist
    if (!Directory.Exists(CONF_LOC))
    {
        Directory.CreateDirectory(CONF_LOC);
    }

    // Load database settings
    DBConfig dbconfig;
    if (File.Exists(CONF_LOC + DBCONF_FILE))
    {
        // If there is a config, read it
        dbconfig = await JsonSerializer.DeserializeAsync<DBConfig>(File.OpenRead(CONF_LOC + DBCONF_FILE));
    }
    else
    {
        // Otherwise create a config with default values and write it to the location
        dbconfig = new DBConfig()
        {
            Database = "database",
            Host = "host",
            Password = "password",
            Username = "user"
        };

        File.WriteAllText(CONF_LOC + DBCONF_FILE, JsonSerializer.Serialize(dbconfig));
        Console.WriteLine("Error: No DB config was found. Creating file...");
    }

}

VooperDB.DbFactory = VooperDB.GetDbFactory();

using var dbctx = VooperDB.DbFactory.CreateDbContext();

string sql = VooperDB.GenerateSQL();

try
{
    await File.WriteAllTextAsync("../Database/Definitions.sql", sql);
}
catch (Exception e)
{

}

VooperDB.RawSqlQuery<string>(sql, null, true);

await DBCache.LoadAsync();

await VoopAI.Main();

builder.Services.AddDbContextPool<VooperDB>(options =>
{
    options.UseNpgsql(VooperDB.ConnectionString, options => options.EnableRetryOnFailure());
});

builder.Services.AddHostedService<EconomyWorker>();
builder.Services.AddHostedService<TransactionWorker>();
builder.Services.AddHostedService<ItemTradeWorker>();
builder.Services.AddHostedService<TimeWorker>();
builder.Services.AddHostedService<DistrictUpdateWorker>();

builder.Services.AddDataProtection().PersistKeysToDbContext<VooperDB>();

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.MaxAge = TimeSpan.FromDays(90);
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Expiration = TimeSpan.FromDays(150);
    options.SlidingExpiration = true;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(90);
    options.Cookie.MaxAge = TimeSpan.FromDays(90);
    options.Cookie.IsEssential = true;
});

builder.Services.AddMvc().AddSessionStateTempDataProvider();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");

//BaseAPI       .AddRoutes(app);
ItemAPI        .AddRoutes(app);
EcoAPI         .AddRoutes(app);
EntityAPI      .AddRoutes(app);
DevAPI         .AddRoutes(app);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ensure districts & Vooperia are created
await VooperDB.Startup();
//await ResourceManager.Load();

await GameDataManager.Load();

ProvinceManager.LoadMap();

foreach (var onaction in GameDataManager.LuaOnActions[SV2.Scripting.LuaObjects.OnActionType.OnServerStart]) {
    // OnServerStart actions MUST change scope
    onaction.EffectBody.Execute(new(null, null));
}

app.Run();
