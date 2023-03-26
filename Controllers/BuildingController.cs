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
using Valour.Shared;
using SV2.Database.Models.Districts;
using SV2.Database.Models.Buildings;

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
    public IActionResult MyBuildings() {
        var user = HttpContext.GetUser();

        List<long> canbuildasids = new() { user.Id };
        canbuildasids.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.ManageBuildings)).Select(x => x.Id).ToList());

        var buildings = DBCache.GetAllProducingBuildings().Where(x => canbuildasids.Contains(x.OwnerId));

        return View(buildings.ToList());
    }

    [HttpGet("/Building/Manage/{id}")]
    [UserRequired]
    public IActionResult Manage(long id) 
    {
        var user = HttpContext.GetUser();
        var building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == id);
    
        if (!(building.OwnerId == user.Id || (building.Owner.EntityType != EntityType.User && ((Group)building.Owner).HasPermission(user, GroupPermissions.ManageBuildings))))
        {    
            StatusMessage = "You lack permission to manage this building!";
            return Redirect("/");
        }

        List<BaseEntity> canbuildas = new() { user };
        canbuildas.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.Build)).Select(x => (BaseEntity)x).ToList());

        var model = new CreateBuildingRequestModel() {
            Province = building.Province,
            LuaBuildingObj = building.BuildingObj,
            ProvinceId = building.ProvinceId,
            BuildingId = building.LuaBuildingObjId,
            AlreadyExistingBuildingId = building.Id,
            CanBuildAs = canbuildas.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList(),
            IncludeScript = true,
            PrefixForIds = "",
            User = user
        };

        return View(new BuildingManageModel() {
            Building = building,
            Name = building.Name,
            Description = building.Description,
            RecipeId = building.RecipeId,
            BuildingId = building.Id,
            createBuildingRequestModel = model,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [UserRequired]
    public IActionResult Manage(BuildingManageModel model) {
        var user = HttpContext.GetUser();
        var building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == model.BuildingId);

        if (!(building.OwnerId == user.Id || (building.Owner.EntityType != EntityType.User && building.Owner.HasPermission(user, GroupPermissions.ManageBuildings)))) {
            StatusMessage = "You lack permission to manage this building!";
            return Redirect("/");
        }

        building.Name = model.Name;
        building.Description = model.Description;
        building.RecipeId = model.RecipeId;

        return RedirectBack($"Successfully updated {model.Name}'s info");
    }

    [HttpPost]
    [UserRequired]
    public async Task<JsonResult> Construct(long buildingrequestid, int levelstobuild) 
    {
        // TODO: after we migrate to dotnet 8 with mixing of blazor and razor, update this method to use json for returning rather than "-&-"
        var buildingrequest = await _dbctx.BuildingRequests.FindAsync(buildingrequestid);
        if (!buildingrequest.Reviewed)
            return Json(new TaskResult<long>(false, "This request has not been reviewed yet!", buildingrequestid));
        if (!buildingrequest.Granted)
            return Json(new TaskResult<long>(false, "This request was not granted! However, the province's governor can change this decision, so try contacting them.", buildingrequestid));

        if (buildingrequest.LevelsBuilt + levelstobuild > buildingrequest.LevelsRequested)
            return Json(new TaskResult<long>(false, "You can not construct more levels than you were approved for!", buildingrequestid));

        var user = HttpContext.GetUser();

        if (buildingrequest.RequesterId != user.Id) {
            Group group = DBCache.Get<Group>(buildingrequest.RequesterId);
            if (!group.HasPermission(user, GroupPermissions.Build)) {
                return Json(new TaskResult<long>(false, "You lack permission to build as this group!", buildingrequestid));
            }
        }
        var buildas = BaseEntity.Find(buildingrequest.RequesterId);

        LuaBuilding luabuildingobj = GameDataManager.BaseBuildingObjs[buildingrequest.BuildingObjId];

        ProducingBuilding? building = null;
        if (buildingrequest.BuildingId is not null)
            building = DBCache.ProvincesBuildings[buildingrequest.ProvinceId].FirstOrDefault(x => x.Id == (long)buildingrequest.BuildingId);
        TaskResult<ProducingBuilding> result = await luabuildingobj.Build(buildas, user, buildingrequest.Province.District, buildingrequest.Province, levelstobuild, building);
        string message = result.Message;
        if (result.Success) {
            buildingrequest.LevelsBuilt += levelstobuild;
            buildingrequest.BuildingId = result.Data.Id;
            if (buildingrequest.BuildingName is not null)
                result.Data.Name = buildingrequest.BuildingName;
            await _dbctx.SaveChangesAsync();
            message += $"Click <a target='_blank' href='/Building/Manage/{result.Data.Id}'>here</a> to view the building.";
        }
        return Json(new TaskResult<long>(result.Success, message + (buildingrequest.LevelsBuilt == buildingrequest.LevelsRequested ? "|REACHEDLIMIT" : ""), buildingrequestid));
    }

    [UserRequired]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async ValueTask<JsonResult> Build(CreateBuildingRequestModel model) {
        Province? province = DBCache.Get<Province>(model.ProvinceId);
        if (province is null)
            return Json(new TaskResult(false, "Province is null"));

        if (!GameDataManager.BaseBuildingObjs.ContainsKey(model.BuildingId))
            return Json(new TaskResult(false, "Building type not found! Please try again."));

        LuaBuilding luabuildingobj = GameDataManager.BaseBuildingObjs[model.BuildingId];

        var user = HttpContext.GetUser();

        if (model.BuildAsId is not null && model.BuildAsId != user.Id) {
            Group group = DBCache.Get<Group>(model.BuildAsId);
            if (!group.HasPermission(user, GroupPermissions.Build)) {
                return Json(new TaskResult(false, "You lack permission to build as this group!"));
            }
        }

        if (model.AlreadyExistingBuildingId is not null) {
            var building = DBCache.ProvincesBuildings[model.ProvinceId].FirstOrDefault(x => x.Id == (long)model.AlreadyExistingBuildingId);

            if (building.OwnerId != user.Id) {
                Group group = DBCache.Get<Group>(building.OwnerId);
                if (!group.HasPermission(user, GroupPermissions.Build)) {
                    return Json(new TaskResult(false, "You lack permission to build as this group!"));
                }
            }
            model.BuildAsId = building.OwnerId;
        }

        if (luabuildingobj.OnlyGovernorCanBuild || province.CanManageBuildingRequests(user)) {
            var buildas = BaseEntity.Find(model.BuildAsId);
            ProducingBuilding? building = null;
            if (model.AlreadyExistingBuildingId is not null) {
                building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == model.AlreadyExistingBuildingId);
            }
            
            TaskResult<ProducingBuilding> result = await luabuildingobj.Build(buildas, user, province.District, province, model.levelsToBuild, building);
            if (!result.Success)
                return Json(new TaskResult(result.Success, result.Message));
            if (model.AlreadyExistingBuildingId is null)
                result.Data.Name = model.Name;
            if (model.AlreadyExistingBuildingId is null)
                return Json(new TaskResult(true, $@"Successfully built {model.levelsToBuild} of {result.Data.BuildingObj.PrintableName}.Click <a href=""/Building/Manage/{result.Data.Id}"">Here</a> to view"));
            else
                return Json(new TaskResult(true, $@"Successfully built {model.levelsToBuild} of {result.Data.BuildingObj.PrintableName}."));
        }

        else {
            var request = new BuildingRequest() {
                Id = IdManagers.GeneralIdGenerator.Generate(),
                RequesterId = (long)model.BuildAsId,
                ProvinceId = model.ProvinceId,
                BuildingId = model.AlreadyExistingBuildingId,
                BuildingObjId = model.BuildingId,
                LevelsRequested = model.levelsToBuild,
                Applied = DateTime.UtcNow,
                Reviewed = false,
                Granted = false,
                LevelsBuilt = 0,
                BuildingName = model.Name
            };

            if (model.AlreadyExistingBuildingId is not null)
                request.BuildingName = model.Name;

            _dbctx.BuildingRequests.Add(request);
            await _dbctx.SaveChangesAsync();

            return Json(new TaskResult(true, "Successfully created and sent your building request."));
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}