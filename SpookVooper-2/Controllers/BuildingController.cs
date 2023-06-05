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
using SV2.Models.Groups;

namespace SV2.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
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
    public async Task<IActionResult> MyBuildings() {
        var user = HttpContext.GetUser();

        List<long> canbuildasids = new() { user.Id };
        canbuildasids.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.ManageBuildings)).Select(x => x.Id).ToList());

        var buildings = DBCache.GetAllProducingBuildings().Where(x => canbuildasids.Contains(x.OwnerId));

        // filiter for jacob
        if (user.ValourId == 12201879245422592)
        {
            var jacobsjoinedgroups = (await user.GetJoinedGroupsAsync()).ToList();
            var newbuildings = new List<ProducingBuilding>();
            foreach (var building in buildings)
            {
                var owner = building.Owner;
                if (owner.EntityType == EntityType.Group)
                {
                    var groupowner = (Group)owner;
                    var district = DBCache.Get<District>(groupowner.Id);
                    if (district is not null)
                    {
                        if (district.Name != "New Vooperis")
                            continue;
                    }

                    var state = DBCache.Get<State>(groupowner.Id);
                    if (state is not null)
                    {
                        if (state.District.Name != "New Vooperis")
                        {
                            if (state.GovernorId is not null && state.Governor.EntityType == EntityType.Group)
                            {
                                if (!jacobsjoinedgroups.Any(x => x.Id == state.GovernorId))
                                {
                                    // if the governor is NOT in any groups joined by Jacob
                                    continue;
                                }
                            }
                        }
                    }
                }
                newbuildings.Add(building);
            }
            buildings = newbuildings;
        }

        return View(buildings.ToList());
    }

    [HttpGet("/Building/Manage/{id}")]
    [UserRequired]
    public IActionResult Manage(long id) 
    {
        var user = HttpContext.GetUser();
        var building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == id);

        if (building is null)
        {
            return RedirectBack("Could not find building!");
        }
    
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

        if (!(building.OwnerId == user.Id || (building.Owner.EntityType != EntityType.User && ((Group)building.Owner).HasPermission(user, GroupPermissions.ManageBuildings)))) {
            StatusMessage = "You lack permission to manage this building!";
            return Redirect("/");
        }

        var recipeidbefore = building.RecipeId;
        building.Name = model.Name;
        building.Description = model.Description;
        building.RecipeId = model.RecipeId;

        if (recipeidbefore != model.RecipeId)
        {
            building.District.UpdateModifiers();
            building.UpdateModifiers();
        }

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
            model.Name = building.Name;
        }

        if (luabuildingobj.OnlyGovernorCanBuild && province.CanManageBuildingRequests(user)) {
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

            _dbctx.BuildingRequests.Add(request);
            await _dbctx.SaveChangesAsync();

            return Json(new TaskResult(true, "Successfully created and sent your building request."));
        }
    }

    [HttpGet("/Building/{buildingid}/upgrade/{upgradeid}")]
    [UserRequired]
    public async Task<IActionResult> BuildingUpgrade(long buildingid, string upgradeid)
    {
        SVUser user = HttpContext.GetUser();

        ProducingBuilding building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == buildingid);

        if (building == null)
            return NotFound($"Error: Could not find {buildingid}");

        if (building.OwnerId != user.Id)
        {
            Group group = DBCache.Get<Group>(building.OwnerId);
            if (!group.HasPermission(user, GroupPermissions.Build))
                return RedirectBack("You lack permission to build as this group!");
        }

        LuaBuildingUpgrade luaupgradeobj = GameDataManager.BaseBuildingUpgradesObjs[upgradeid];
        BuildingUpgrade? upgrade = building.Upgrades.FirstOrDefault(x => x.LuaBuildingUpgradeId == luaupgradeobj.Id);

        TaskResult<ProducingBuilding> result = await luaupgradeobj.Build(building.Owner, user, building.District, building.Province, 1, building, upgrade);

        building.UpdateModifiers();
        return RedirectBack(result.Message);
    }

    [HttpGet("/Building/TransferBuilding/{buildingid}")]
    [UserRequired]
    public IActionResult TransferBuilding(long buildingid)
    {
        SVUser user = HttpContext.GetUser();

        ProducingBuilding building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == buildingid);

        if (building == null)
            return NotFound($"Error: Could not find {buildingid}");

        if (building.Owner.EntityType == EntityType.User && building.OwnerId != user.Id)
            return RedirectBack("You must be owner of the building to transfer it!");

        if (building.Owner.EntityType == EntityType.Group)
        {
            var group = (Group)building.Owner;
            if (!group.IsOwner(user))
                return RedirectBack("You must be owner of the group that owns the building to transfer it! Or the owner of the group that owns the group, and so on!");
        }

        TransferBuildingModel model = new TransferBuildingModel()
        {
            User = user,
            Building = building
        };

        return View(model);
    }

    [HttpPost("/Building/TransferBuilding/{buildingid}")]
    [ValidateAntiForgeryToken]
    [UserRequired]
    public IActionResult TransferBuilding(long buildingid, long EntityId)
    {
        var user = HttpContext.GetUser();
        ProducingBuilding building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.Id == buildingid);

        if (building == null)
            return NotFound($"Error: Could not find {buildingid}");

        if (building.Owner.EntityType == EntityType.User && building.OwnerId != user.Id)
            return RedirectBack("You must be owner of the building to transfer it!");

        if (building.Owner.EntityType == EntityType.Group)
        {
            var group = (Group)building.Owner;
            if (!group.IsOwner(user))
                return RedirectBack("You must be owner of the group that owns the building to transfer it! Or the owner of the group that owns the group, and so on!");
        }

        var toentity = BaseEntity.Find(EntityId);

        if (toentity is null)
            return RedirectBack("To Entity not found!");

        building.OwnerId = toentity.Id;

        StatusMessage = $"Successfully transferred building ownership to {toentity.Name}";
        return Redirect("/Building/MyBuildings");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}