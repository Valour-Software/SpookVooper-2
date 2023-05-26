using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using SV2.Web;
using SV2.Database.Models.Economy;
using Microsoft.EntityFrameworkCore;

namespace SV2.API
{
    public class DevAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            //app.MapGet   ("api/dev/database/sql", GetSQL);
            app.MapGet("api/dev/lackaccess", LackAccess);
            app.MapGet("api/dev/getdistrictproduction", GetDistrictProduction);
        }

        private static async Task LackAccess(HttpContext ctx) {
            await ctx.Response.WriteAsync("You lack access to SV 2.0. SV 2.0 is currently in private early alpha, public early alpha is expected in a few weeks to months.");
        }

        private static async Task GetSQL(HttpContext ctx, VooperDB db, bool drop = false)
        {
            if (drop && false) {
                VooperDB.Instance.Database.EnsureDeleted();
                VooperDB.Instance.Database.EnsureCreated();
                await VooperDB.Instance.SaveChangesAsync();
            }
            await ctx.Response.WriteAsync(VooperDB.GenerateSQL());
        }

        private static async Task GetDistrictProduction(HttpContext ctx, VooperDB db, string name,string resource)
        {
            var district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name == name);
            if (district is null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find district with name of {name}!");
                return;
            }

            // / 10550.0 * buildingobj.Recipes.First().PerHour * buildingobj.Recipes.First().Outputs.First().Value

            var buildingobj = GameDataManager.BaseBuildingObjs.Values.First(x => x.MustHaveResource == resource);
            var second_part = buildingobj.Recipes.First().PerHour * buildingobj.Recipes.First().Outputs.First().Value;
            double sum = 0;
            foreach (var province in district.Provinces)
            {
                sum += province.GetMiningResourceProduction(resource) / 10550.0 * second_part;
            }

            await ctx.Response.WriteAsync(sum.ToString());
        }
    }
}