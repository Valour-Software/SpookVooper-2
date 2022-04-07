global using SV2.Database;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SV2.API;
using SV2.Workers;

var builder = WebApplication.CreateBuilder(args);
string CONF_LOC = "SV2Config/";
string DBCONF_FILE = "DBConfig.json";

// Add services to the container.
builder.Services.AddMvc()
    .AddRazorRuntimeCompilation();

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

builder.Services.AddDbContextPool<VooperDB>(options =>
{
    options.UseNpgsql(VooperDB.ConnectionString, options => options.EnableRetryOnFailure());
});

builder.Services.AddHostedService<EconomyWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//BaseAPI       .AddRoutes(app);
EntityAPI     .AddRoutes(app);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
