using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;

namespace SV2.API
{
    public class EcoAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/eco/{svid}/credits", GetCredits);
        }

        private static async Task GetCredits(HttpContext ctx, VooperDB db, string svid)
        {
            IEntity account = await IEntity.FindAsync(svid);
            if (account == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {svid}");
                return;
            }

            await ctx.Response.WriteAsync(account.Credits.ToString());
        }

    }
}