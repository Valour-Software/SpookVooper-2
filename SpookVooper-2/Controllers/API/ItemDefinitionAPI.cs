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
    }

    private static async Task GetAllAsync(HttpContext ctx)
    {
        var defs = DBCache.GetAll<ItemDefinition>().ToList();
        await ctx.Response.WriteAsJsonAsync(defs);
    }
}