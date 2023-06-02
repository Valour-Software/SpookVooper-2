using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class UserAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/users/{id}", GetAsync).RequireCors("ApiPolicy");
    }

    private static async Task<IResult> GetAsync(HttpContext ctx, long id)
    {
        SVUser? user = SVUser.Find(id);
        if (user is null)
            return ValourResult.NotFound($"Could not find user with id {id}");

        return Results.Json(user);
    }
}