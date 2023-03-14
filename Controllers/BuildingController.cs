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
    
        if (!(building.OwnerId == user.Id || (building.Owner.EntityType != EntityType.User && building.Owner.HasPermission(user, GroupPermissions.ManageBuildings))))
        {    
            StatusMessage = "You lack permission to manage this building!";
            return Redirect("/");
        }
        
        return View(new BuildingManageModel() {
            Building = building,
            Name = building.Name,
            Description = building.Description,
            RecipeId = building.RecipeId,
            BuildingId = building.Id
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
    public async Task<string> Construct(long buildingrequestid, int levelstobuild) 
    {
        var buildingrequest = await _dbctx.BuildingRequests.FindAsync(buildingrequestid);
        if (!buildingrequest.Reviewed)
            return $"{buildingrequestid}-&-This request has not been reviewed yet!";
        if (!buildingrequest.Granted)
            return $"{buildingrequestid}-&-This request was not granted! However, the province's governor can change this decision, so try contacting them.";

        if (buildingrequest.LevelsBuilt + levelstobuild > buildingrequest.LevelsRequested)
            return $"{buildingrequestid}-&-You can not construct more levels than you were approved for!";
;
        var user = HttpContext.GetUser();

        if (buildingrequest.RequesterId != user.Id) {
            Group group = DBCache.Get<Group>(buildingrequest.RequesterId);
            if (!group.HasPermission(user, GroupPermissions.Build)) {
                return $"{buildingrequestid}-&-You lack permission to build as this group!";
            }
        }
        var buildas = BaseEntity.Find(buildingrequest.RequesterId);

        LuaBuilding luabuildingobj = GameDataManager.BaseBuildingObjs[buildingrequest.BuildingObjId];

        ProducingBuilding? building = null;
        if (buildingrequest.BuildingId is not null)
            building = DBCache.ProvincesBuildings[buildingrequest.ProvinceId].FirstOrDefault(x => x.Id == (long)buildingrequest.BuildingId);
        TaskResult<ProducingBuilding> result = await luabuildingobj.Build(buildas, user, buildingrequest.Province.District, buildingrequest.Province, levelstobuild, building);
        if (result.Success) {
            buildingrequest.LevelsBuilt += levelstobuild;
            await _dbctx.SaveChangesAsync();
        }
        return $"{buildingrequestid}-&-{result.Message}";
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
    public async ValueTask<IActionResult> Build(CreateBuildingRequestModel model) {
        Province? province = DBCache.Get<Province>(model.ProvinceId);
        if (province is null)
            return Redirect("/");

        if (!GameDataManager.BaseBuildingObjs.ContainsKey(model.BuildingId))
            return RedirectBack("Building type not found! Please try again.");

        LuaBuilding luabuildingobj = GameDataManager.BaseBuildingObjs[model.BuildingId];

        var user = HttpContext.GetUser();

        if (model.BuildAsId is not null && model.BuildAsId != user.Id) {
            Group group = DBCache.Get<Group>(model.BuildAsId);
            if (!group.HasPermission(user, GroupPermissions.Build)) {
                return RedirectBack("You lack permission to build as this group!");
            }
        }

        if (model.AlreadyExistingBuildingId is not null) {
            var building = DBCache.ProvincesBuildings[model.ProvinceId].FirstOrDefault(x => x.Id == (long)model.AlreadyExistingBuildingId);

            if (building.OwnerId != user.Id) {
                Group group = DBCache.Get<Group>(building.OwnerId);
                if (!group.HasPermission(user, GroupPermissions.Build)) {
                    return RedirectBack("You lack permission to build as this group!");
                }
            }
            model.BuildAsId = building.OwnerId;
        }

        if (luabuildingobj.OnlyGovernorCanBuild) {
            var buildas = BaseEntity.Find(model.BuildAsId);
            ProducingBuilding? building = null;
            if (model.AlreadyExistingBuildingId is not null) {
                building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == model.AlreadyExistingBuildingId);
            }
            
            TaskResult<ProducingBuilding> result = await luabuildingobj.Build(buildas, user, province.District, province, model.levelsToBuild, building);
            StatusMessage = result.Message;
            if (!result.Success)
                return RedirectBack();
            return Redirect($"/Building/Manage/{result.Data.Id}");
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
                LevelsBuilt = 0
            };

            _dbctx.BuildingRequests.Add(request);
            await _dbctx.SaveChangesAsync();

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