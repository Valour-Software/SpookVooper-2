using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class RecipeAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/recipes/getall", GetAllAsync).RequireCors("ApiPolicy");
    }

    private static async Task GetAllAsync(HttpContext ctx)
    {
        var recipes = GameDataManager.BaseRecipeObjs.Values.ToList();
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(recipes));
    }
}