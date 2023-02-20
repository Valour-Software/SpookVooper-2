using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Helpers;
using SV2.Extensions;

namespace SV2.Controllers;

public class ProvinceController : SVController
{
    private readonly ILogger<ProvinceController> _logger;
    private readonly VooperDB _dbctx;

    public ProvinceController(ILogger<ProvinceController> logger,
        VooperDB dbctx)
    {
        _logger = logger;
        _dbctx = dbctx;
    }

    [HttpGet("/Province/View/{id}")]
    public IActionResult View(long id)
    {
        if (!DBCache.HCache[typeof(Province)].TryGetValue(id, out object _obj))
            return Redirect("/");
        Province province = (Province)_obj;

        return View(province);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}