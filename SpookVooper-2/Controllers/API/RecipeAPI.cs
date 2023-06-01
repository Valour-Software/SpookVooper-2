using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class RecipeAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/recipes/getall", GetAllAsync).RequireCors("ApiPolicy");
    }

    private static async Task<IResult> GetAllAsync(HttpContext ctx)
    {
        return Results.Json(GameDataManager.BaseRecipeObjs.Values);
    }
}