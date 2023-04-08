using Microsoft.AspNetCore.Mvc;
using SV2.Helpers;

namespace SV2.Controllers;

public class MiscController : SVController
{
    public IActionResult NetResourcesForRecipes()
    {
        return View();
    }
}
