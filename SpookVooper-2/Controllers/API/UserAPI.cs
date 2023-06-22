using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;
using SV2.Extensions;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class UserAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/users/{id}", GetAsync).RequireCors("ApiPolicy");
        app.MapGet   ("api/users/self", GetSelfAsync).RequireCors("ApiPolicy");
    }

    private static async Task<IResult> GetAsync(HttpContext ctx, long id)
    {
        SVUser? user = SVUser.Find(id);
        if (user is null)
            return ValourResult.NotFound($"Could not find user with id {id}");

        return Results.Json(user);
    }

    private static async Task<IResult> GetSelfAsync(HttpContext ctx)
    {
        SVUser? user = ctx.GetUser();
        if (user is null)
            return ValourResult.NotFound($"Could not find your account! Try logging in or relogging in.");

        return Results.Json(user);
    }
}