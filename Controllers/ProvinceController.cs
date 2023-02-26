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

    [HttpGet("/Province/Edit/{id}")]
    public IActionResult Edit(long id)
    {
        if (!DBCache.HCache[typeof(Province)].TryGetValue(id, out object _obj))
            return Redirect("/");
        Province province = (Province)_obj;
        SVUser? user = UserManager.GetUser(HttpContext);

        if (user is null)
            return Redirect("/account/login");
        if (!province.CanEdit(user))
            return RedirectBack("You lack permission to manage this province!");

        return View(province);
    }

    [HttpGet("/Province/ChangeGovernor/{id}")]
    public IActionResult ChangeGovernor(long id, long GovernorId)
    {
        Province? province = DBCache.Get<Province>(id);
        if (province is null)
            return Redirect("/");
        SVUser? user = UserManager.GetUser(HttpContext);

        if (user is null)
            return Redirect("/account/login");
        if (province.District.GovernorId != user.Id)
            return RedirectBack("You must be governor of the district to change the governor of a province!");

        province.GovernorId = GovernorId;

        return RedirectBack($"Successfully changed the governorship of this province to {BaseEntity.Find(GovernorId).Name}");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}