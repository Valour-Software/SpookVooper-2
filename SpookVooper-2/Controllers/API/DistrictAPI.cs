using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class DistrictAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/districts/{id}", GetDistrictAsync).RequireCors("ApiPolicy");
        app.MapGet   ("api/getallprovinces", GetAllProvincesAsync).RequireCors("ApiPolicy");
    }

    private static async Task GetDistrictAsync(HttpContext ctx, long id)
    {
        await ctx.Response.WriteAsJsonAsync(DBCache.Get<District>(id));
    }

    private static async Task GetAllProvincesAsync(HttpContext ctx)
    {
        await ctx.Response.WriteAsJsonAsync(DBCache.GetAll<Province>().ToList());
    }
}