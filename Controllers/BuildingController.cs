using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Helpers;
using SV2.Extensions;
using SV2.Database.Managers;
using SV2.Models.Provinces;
using SV2.Models.Building;
using SV2.Scripting.LuaObjects;

namespace SV2.Controllers;

public class BuildingController : SVController
{
    private readonly ILogger<BuildingController> _logger;
    private readonly VooperDB _dbctx;

    public BuildingController(ILogger<BuildingController> logger,
        VooperDB dbctx)
    {
        _logger = logger;
        _dbctx = dbctx;
    }

    [HttpGet]
    [UserRequired]
    public IActionResult Build(string buildingid, long provinceid)
    {
        Province? province = DBCache.Get<Province>(provinceid);
        if (province is null)
            return RedirectBack("Province not found! Please try again.");

        if (!GameDataManager.BaseBuildingObjs.ContainsKey(buildingid))
            return RedirectBack("Building type not found! Please try again.");

        LuaBuilding luabuildingobj = GameDataManager.BaseBuildingObjs[buildingid];

        //Console.WriteLine(GameDataManager.BaseRecipeObjs.FirstOrDefault());

        var user = HttpContext.GetUser();

        List<BaseEntity> canbuildas = new() { user };
        canbuildas.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.Build)).Select(x => (BaseEntity)x).ToList());

        return View(new CreateBuildingRequestModel() { 
            Province = province, 
            LuaBuildingObj = luabuildingobj,
            ProvinceId = province.Id,
            BuildingId = buildingid,
            CanBuildAs = canbuildas.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList()
        });
    }

    [UserRequired]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult Build(CreateBuildingRequestModel model) {
        Province? province = DBCache.Get<Province>(model.ProvinceId);
        if (province is null)
            return Redirect("/");

        if (!GameDataManager.BaseBuildingObjs.ContainsKey(model.BuildingId))
            return RedirectBack("Building type not found! Please try again.");

        LuaBuilding luabuildingobj = GameDataManager.BaseBuildingObjs[model.BuildingId];

        var user = HttpContext.GetUser();

        if (model.BuildAsId != user.Id) {
            Group group = DBCache.Get<Group>(model.BuildAsId);
            if (!group.HasPermission(user, GroupPermissions.Build)) {
                return RedirectBack("You lack permission to build as this group!");
            }
        }

        if (luabuildingobj.OnlyGovernorCanBuild) {
            var buildas = BaseEntity.Find(model.BuildAsId);
            var result = 
            if (luabuildingobj)
            StatusMessage = $"Successfully built {model.levelsToBuild} levels of {luabuildingobj.PrintableName}.";
            return Redirect($"/Province/Build/{model.ProvinceId}");
        }

        else {
            var request = new BuildingRequest() {
                Id = IdManagers.GeneralIdGenerator.Generate(),
                RequesterId = model.BuildAsId,
                ProvinceId = model.ProvinceId,
                BuildingId = null,
                BuildingObjId = model.BuildingId,
                LevelsRequested = model.levelsToBuild,
                Applied = DateTime.UtcNow,
                Reviewed = false,
                Granted = false
            };

            _dbctx.BuildingRequests.Add(request);
            _dbctx.SaveChangesAsync();

            StatusMessage = "Successfully created and sent your building request.";
            return Redirect($"/Province/Build/{model.ProvinceId}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}