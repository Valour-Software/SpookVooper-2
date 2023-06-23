using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class ItemDefinitionAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/itemdefinitions/all", GetAllAsync).RequireCors("ApiPolicy");
        app.MapGet   ("api/itemdefinitions/{defid}", GetAsync).RequireCors("ApiPolicy");
    }

    private static async Task GetAsync(HttpContext ctx, long defid)
    {
        ItemDefinition? itemdef = DBCache.Get<ItemDefinition>(defid);
        if (itemdef is null)
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsync($"Could not find item definition with id {defid}");
            return;
        }

        await ctx.Response.WriteAsJsonAsync(itemdef);
    }

    private static async Task GetAllAsync(HttpContext ctx)
    {
        var defs = DBCache.GetAll<ItemDefinition>().ToList();
        await ctx.Response.WriteAsJsonAsync(defs);
    }
}