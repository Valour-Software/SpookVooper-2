global using SV2.Database;
global using SV2.Database.Models.Districts;
global using SV2.Database.Models.Economy;
global using SV2.Database.Models.Entities;
global using SV2.Database.Models.Factories;
global using SV2.Database.Models.Government;
global using SV2.Database.Models.Groups;
global using SV2.Database.Models.Items;
global using SV2.Database.Models.Military;
global using Shared.Models.Permissions;
global using SV2.Database.Models.Buildings;
global using SV2.Database.Models.Users;
global using SV2.Database.Models.OAuth2;
global using SV2.Models.Districts;
global using SV2.Managers;
global using System.Net.Http.Json;
global using Valour.Net.Client;
global using Valour.Api.Models.Economy;
global using SV2.Http;
global using Shared.Models.TradeDeals;
global using ProvinceModifierType = Shared.Models.Districts.ProvinceModifierType;
global using DistrictModifierType = Shared.Models.Districts.Modifiers.DistrictModifierType;
global using ProvinceMetadata = Shared.Models.Districts.ProvinceMetadata;
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
using Microsoft.OpenApi.Models;
using SV2.Web;

//LuaParser parser = new();

//parser.LoadTokenizer();
//parser.Parse(File.ReadAllText("Managers/Data/BuildingUpgrades/factoryupgrades.lua"), "factoryupgrades.lua");

//string jsonString = JsonSerializer.Serialize((LuaTable)parser.Objects.Items.First(), options: new() { WriteIndented = true});
//await File.WriteAllTextAsync("Managers/Data/ParserOutput.txt", jsonString);

Defines.Load();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "ApiPolicy",
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowAnyOrigin();
        });
});

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

//builder.Services.AddMvc(options =>
//{
//    options.Filters.Add<UserRequiredAttribute>();
//});

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SpookVooper API", Description = "The official SpookVooper API", Version = "v1.0" });
    c.AddSecurityDefinition("Apikey", new OpenApiSecurityScheme()
    {
        Description = "The apikey used for authorizing your account.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Apikey"
    });
});

builder.Services.AddHostedService<EconomyWorker>();
builder.Services.AddHostedService<TransactionWorker>();
builder.Services.AddHostedService<ItemTradeWorker>();
builder.Services.AddHostedService<TimeWorker>();
builder.Services.AddHostedService<DistrictUpdateWorker>();
builder.Services.AddHostedService<VoopAIWorker>();
builder.Services.AddHostedService<StatWorker>();

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

builder.Services.AddMvc().AddSessionStateTempDataProvider().AddRazorRuntimeCompilation();

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

app.UseSwagger();

app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();

app.UseCors();

//BaseAPI       .AddRoutes(app);
ItemAPI.AddRoutes(app);
EcoAPI.AddRoutes(app);
EntityAPI.AddRoutes(app);
DevAPI.AddRoutes(app);
BuildingAPI.AddRoutes(app);
RecipeAPI.AddRoutes(app);
DistrictAPI.AddRoutes(app);
UserAPI.AddRoutes(app);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// ensure districts & Vooperia are created
await VooperDB.Startup();
//await ResourceManager.Load();

await GameDataManager.Load();

ProvinceManager.LoadMap();

foreach (var onaction in GameDataManager.LuaOnActions[SV2.Scripting.LuaObjects.OnActionType.OnServerStart]) {
    // OnServerStart actions MUST change scope
    onaction.EffectBody.Execute(new(null, null));
}

List<BaseEntity> entities = new();
//entities.AddRange(DBCache.GetAll<SVUser>());
//entities.AddRange(DBCache.GetAll<Group>());

// Migration district & province populations to be user based
if (true)
{
    foreach (var district in DBCache.GetAll<District>())
    {
        district.BasePopulationFromUsers = 2_500_000.0 * district.Citizens.Count;

        // handle provinces (this is the real fun part)
        var totalPrevProvincePopulation = district.Provinces.Sum(x => x.Population);
        var ratio = totalPrevProvincePopulation / district.BasePopulationFromUsers;
        foreach (var province in district.Provinces)
        {
            province.PopulationMultiplier = province.Population / district.BaseProvincePopulation / ratio;
        }
    }
}

Console.WriteLine("Migrating Eco");
Console.WriteLine($"Total Entites to migrate: {entities.Count}");
int i = 0;
foreach (var entity in entities)
{
    i += 1;
    if (entity.EcoAccountId == 0)
    {
        await entity.Create();
        await Task.Delay(210);
    }
    Console.WriteLine($"Migrated {i}/{entities.Count}");
}

foreach (var state in DBCache.GetAll<State>())
{
    if (state.GovernorId is not null)
    {
        BaseEntity entity = BaseEntity.Find(state.GovernorId);
        if (!state.Group.MembersIds.Contains((long)state.GovernorId))
            state.Group.MembersIds.Add((long)state.GovernorId);

        state.Group.AddEntityToRole(DBCache.Get<Group>(100), entity, state.Group.Roles.First(x => x.Name == "Governor"), true);
    }
}

app.Run();
