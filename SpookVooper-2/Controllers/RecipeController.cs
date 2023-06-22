using Microsoft.AspNetCore.Mvc;
using SV2.Models.Recipes;

namespace SV2.Controllers;

public class RecipeController : Controller
{
    public async Task<IActionResult> CreateNewRecipe(long entityid, string baserecipeid)
    {
        return View(new CreateNewRecipeModel()
        {
            EntityId = entityid,
            BaseRecipeId = baserecipeid
        });
    }
}
