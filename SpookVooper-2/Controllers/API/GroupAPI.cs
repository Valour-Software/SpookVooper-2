using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using Microsoft.AspNetCore.Cors;
using SV2.Helpers;
using SV2.Extensions;
using Microsoft.AspNetCore.Http;

namespace SV2.API;

[EnableCors("ApiPolicy")]
public class GroupAPI : BaseAPI
{
    public static void AddRoutes(WebApplication app)
    {
        app.MapGet   ("api/groups/{id}", GetAsync).RequireCors("ApiPolicy");
        app.MapGet   ("api/groups/mine/all/withperm/{permissionname}", MineAllWithPerm).RequireCors("ApiPolicy").AddEndpointFilter<UserRequiredAttribute>();
    }

    private static async Task<IResult> GetAsync(HttpContext ctx, long id)
    {
        Group? group = Group.Find(id);
        if (group is null)
            return ValourResult.NotFound($"Could not find group with id {id}");

        return Results.Json(group);
    }

    private static async Task<IResult> MineAllWithPerm(HttpContext ctx, string permissionname)
    {
        var user = ctx.GetUser();

        var permission = GroupPermissions.Permissions.FirstOrDefault(x => x.Name == permissionname);
        if (permission is null) {
            return ValourResult.BadRequest($"Could not find group permission with name {permissionname}!");
        }

        return Results.Json(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, permission)).ToList());
    }

}