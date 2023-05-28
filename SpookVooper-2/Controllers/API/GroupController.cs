using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class GroupAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/groups/{id}", GetAsync).RequireCors("ApiPolicy");
    }

    private static async Task<IResult> GetAsync(HttpContext ctx, long id)
    {
        Group? group = Group.Find(id);
        if (group is null)
            return ValourResult.NotFound($"Could not find group with id {id}");

        return Results.Json(group);
    }
}