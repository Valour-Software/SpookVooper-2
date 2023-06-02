using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using Microsoft.AspNetCore.Cors;

namespace SV2.API
{
    [EnableCors("ApiPolicy")]
    public class ItemAPI : BaseAPI
    {
        private static IdManager idManager = new(1);
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/item/{itemid}", GetItem).RequireCors("ApiPolicy");
            app.MapGet   ("api/items/{itemdefid}/give", Give).RequireCors("ApiPolicy");
            app.MapGet   ("api/items/{itemdefid}/ownership/{entityid}", GetOwnership).RequireCors("ApiPolicy");
            app.MapGet   ("api/item/{itemid}/owner", GetOwner).RequireCors("ApiPolicy");
            app.MapGet   ("api/definition/{definitionid}/items", GetItemsFromDefinition).RequireCors("ApiPolicy");
            app.MapGet   ("api/items/{entityid/getallitemsowned}", GetAllItemsOwned).RequireCors("ApiPolicy");
        }

        private static async Task GetAllItemsOwned(HttpContext ctx, long entityid)
        {
            var items = DBCache.GetAll<SVItemOwnership>().Where(x => x.OwnerId == entityid).ToList();

            await ctx.Response.WriteAsJsonAsync(items);
        }

        private static async Task GetItemsFromDefinition(HttpContext ctx, VooperDB db, long definitionid)
        {
            IEnumerable<SVItemOwnership> definitions = DBCache.GetAll<SVItemOwnership>().Where(x => x.DefinitionId == definitionid);

            await ctx.Response.WriteAsJsonAsync(definitions);
        }

        private static async Task GetItem(HttpContext ctx, VooperDB db, long itemid)
        {
            // find Item
            SVItemOwnership? item = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            await ctx.Response.WriteAsJsonAsync(item);
        }

        private static async Task GetOwner(HttpContext ctx, VooperDB db, long itemid)
        {
            // find Item
            SVItemOwnership? item = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            await ctx.Response.WriteAsJsonAsync(item.Owner);
        }

        private static async Task GetOwnership(HttpContext ctx, long itemdefid, long entityid)
        {
            // find Item
            ItemDefinition? itemdef = DBCache.Get<ItemDefinition>(itemdefid);
            if (itemdef is null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with definition id {itemdefid}");
                return;
            }

            BaseEntity? entity = BaseEntity.Find(entityid);
            if (entity is null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {entityid}!");
                return;
            }

            await ctx.Response.WriteAsync((entity.SVItemsOwnerships.ContainsKey(itemdefid) ? entity.SVItemsOwnerships[itemdefid].Amount : 0).ToString());
        }

        private static async Task Give(HttpContext ctx, VooperDB db, long itemdefid, string apikey, long fromid, long toid, int amount, string detail)
        {
            // find Item
            var def = DBCache.Get<ItemDefinition>(itemdefid);
            if (def is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item definition with definition id {itemdefid}");
                return;
            }

            SVItemOwnership? item = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.OwnerId == fromid && x.DefinitionId == itemdefid);
            if (item is null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item! Maybe fromid does not own any of this item?");
                return;
            }

            // get Entity with the api key
            BaseEntity? entity = await BaseEntity.FindByApiKey(apikey, db);

            if (entity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Invalid api key!");
                return;
            }

            BaseEntity? fromentity = BaseEntity.Find(fromid);
            if (fromentity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {fromid}!");
                return;
            }

            if (entity.Id != fromentity.Id)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"You can not use one entity's api key or oauth key to send a item from another entity!");
                return;
            }

            if (!fromentity.HasPermission(entity, GroupPermissions.Resources))
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"You lack permission to send resources!");
                return;
            }

            // find toentity

            BaseEntity? toentity = BaseEntity.Find(toid);

            if (entity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {toid}!");
                return;
            }

            if (amount <= 0) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Amount must be greater than 0!");
                return;
            }

            ItemTrade trade = new() {
                Id = idManager.Generate(),
                Amount = amount,
                FromId = fromid,
                ToId = toid,
                Time = DateTime.UtcNow,
                DefinitionId = item.DefinitionId,
                Details = detail,
            };

            await ctx.Response.WriteAsync((await trade.Execute()).Info);
        }
    }
}