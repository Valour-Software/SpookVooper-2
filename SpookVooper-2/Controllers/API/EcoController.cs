using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using SV2.Web;
using SV2.Database.Models.Economy;
using Microsoft.AspNetCore.Cors;

namespace SV2.API
{
    [EnableCors("ApiPolicy")]
    public class EcoAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/eco/transaction/send", SendTransaction).RequireCors("ApiPolicy");
        }

        private static async Task SendTransaction(HttpContext ctx, VooperDB db, long fromid, long toid, string apikey, decimal amount, string detail, TransactionType trantype)
        {
            BaseEntity? fromentity = BaseEntity.Find(fromid);
            if (fromentity == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {fromid}");
                return;
            }

            BaseEntity? toentity = BaseEntity.Find(toid);
            if (toentity == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {toid}");
                return;
            }

            // check if the apikey matches the fromentity.Apikey
            // TODO: add checking for oauth key
            
            if (fromentity.ApiKey != apikey) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Invalid api key!");
                return;
            }

            if (amount <= 0) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Amount must be greater than 0!");
                return;
            }

            Transaction tran = new Transaction(fromid, toid, amount, trantype, detail);

            TaskResult result = await tran.Execute();

            await ctx.Response.WriteAsJsonAsync(result);
        }
    }
}