using Microsoft.AspNetCore.Mvc;
using Shared.Models.Entities;
using SV2.Extensions;
using SV2.Helpers;
using SV2.Models.Recipes;
using Valour.Api.Models;

namespace SV2.Controllers;

public class RecipeController : SVController
{
    public async Task<IActionResult> CreateNewRecipe(long entityid, string baserecipeid)
    {
        return View(new CreateNewRecipeModel()
        {
            EntityId = entityid,
            BaseRecipeId = baserecipeid
        });
    }

    [UserRequired]
    public async Task<IActionResult> RecipesThatICanUse()
    {
        var user = HttpContext.GetUser();
        List<long> canbuildasids = new() { user.Id };
        canbuildasids.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.ManageBuildings)).Select(x => x.Id).ToList());
        List<Recipe> recipes = new();

        foreach (var recipe in DBCache.GetAll<Recipe>().Where(x => x.BaseRecipe.Editable))
        {
            if (canbuildasids.Any(x => recipe.CanUse(x)))
                recipes.Add(recipe);
        }

        return View(recipes);
    }

    [UserRequired]
    public async Task<IActionResult> MyRecipes()
    {
        var user = HttpContext.GetUser();
        List<long> canbuildasids = new() { user.Id };
        canbuildasids.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.ManageBuildings)).Select(x => x.Id).ToList());
        return View(DBCache.GetAll<Recipe>().Where(x => x.BaseRecipe.Editable && canbuildasids.Contains(x.OwnerId)).ToList());
    }
}
