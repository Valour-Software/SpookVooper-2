using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;

namespace SV2.API
{
    public class EntityAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/entity/{svid}/name", GetName);
            app.MapGet   ("api/entity/{svid}/credits", GetCredits);
            app.MapGet   ("api/entity/search", Search);
        }

        private static async Task GetName(HttpContext ctx, VooperDB db, long svid)
        {
            BaseEntity? entity = BaseEntity.Find(svid);
            if (entity == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {svid}");
                return;
            }

            await ctx.Response.WriteAsync(entity.Name);
        }

        private static async Task GetCredits(HttpContext ctx, VooperDB db, long svid)
        {
            BaseEntity? account = BaseEntity.Find(svid);
            if (account == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {svid}");
                return;
            }

            await ctx.Response.WriteAsync(account.Credits.ToString());
        }

        private static async Task<IEnumerable<BaseEntity>> Search(string name, int amount = 20)
        {
            List<BaseEntity> entities = new();

            // Cap at 20
            if (amount > 20)
                amount = 20;

            if (name == null)
                return entities;

            name = name.ToLower();

            var users = DBCache.GetAll<SVUser>().Where(x => x.Name.ToLower().Contains(name));
            var groups = DBCache.GetAll<Group>().Where(x => x.Name.ToLower().Contains(name));

            entities.AddRange(users);
            entities.AddRange(groups);

            var top = entities.OrderBy(x => x.Name.ToLower().StartsWith(name.ToLower())).TakeLast(amount).ToList();

            //snaps.Reverse();

            return entities;
        }
    }
}