using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class BuildingAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/buildings/{ownerid}", GetAsync).RequireCors("ApiPolicy");
    }

    private static async Task<IResult> GetAsync(HttpContext ctx, long ownerid)
    {
        var buildings = DBCache.GetAllProducingBuildings().Where(x => x.OwnerId == ownerid).ToList();

        return Results.Json(buildings);
    }
}