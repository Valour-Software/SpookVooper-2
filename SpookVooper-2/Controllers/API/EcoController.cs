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

        private static async Task SendTransaction(HttpContext ctx, VooperDB db, long fromid, long toid, string apikey, decimal amount, string detail, TransactionType trantype, bool? isanexpense = null)
        {
            // get Entity with the api key
            BaseEntity? entity = await BaseEntity.FindByApiKey(apikey, db);

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

            if (entity.Id != fromentity.Id)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"You can not use one entity's api key or oauth key to send a transaction from another entity!");
                return;
            }

            if (!fromentity.HasPermission(entity, GroupPermissions.Eco))
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"You lack permission to send transactions!");
                return;
            }

            if (amount <= 0) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Amount must be greater than 0!");
                return;
            }

            if (isanexpense is not null) 
            {
                // only groups with the CanSetTransactionsExpenseStatus flag can set the isanexpense
                if (toentity.EntityType != EntityType.Group && toentity.EntityType != EntityType.Corporation)
                {
                    ctx.Response.StatusCode = 401;
                    await ctx.Response.WriteAsync("Only groups can use isanexpense!");
                    return;
                }
                Group group = (Group)toentity;
                if (!group.Flags.Contains(GroupFlag.CanSetTransactionsExpenseStatus))
                {
                    ctx.Response.StatusCode = 401;
                    await ctx.Response.WriteAsync("Only groups with the CanSetTransactionsExpenseStatus flag can use isanexpense!");
                    return;
                }
            }

            var tran = new SVTransaction(fromentity, toentity, amount, trantype, detail);
            if (isanexpense is not null)
                tran.IsAnExpense = isanexpense;

            TaskResult result = await tran.Execute();

            await ctx.Response.WriteAsJsonAsync(result);
        }
    }
}