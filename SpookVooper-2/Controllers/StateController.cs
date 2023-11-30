using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Models.States;
using SV2.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Helpers;
using SV2.Extensions;
using SV2.Database.Models.Districts;
using System.Xml.Linq;
using SV2.Database.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV2.Models.Building;

namespace SV2.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class StateController : SVController
{
    private readonly ILogger<StateController> _logger;
    private readonly VooperDB _dbctx;

    public StateController(ILogger<StateController> logger,
        VooperDB dbctx)
    {
        _logger = logger;
        _dbctx = dbctx;
    }

    public IActionResult View(long id) {
        State? state = DBCache.Get<State>(id);
        if (state is null)
            return RedirectBack();

        return View(state);
    }

    [UserRequired]
    public IActionResult ViewBuildings(long id)
    {
        var user = HttpContext.GetUser();
        State? state = DBCache.Get<State>(id);
        if (state is null)
            return RedirectBack();

        var viewbuildingmodel = new StateViewBuildingModel()
        {
            ManageModels = new(),
            State = state
        };

        foreach (var building in state.Provinces.SelectMany(x => x.GetBuildings()))
        {

            var model = new CreateBuildingRequestModel()
            {
                Province = building.Province,
                LuaBuildingObj = building.BuildingObj,
                ProvinceId = building.ProvinceId,
                BuildingId = building.LuaBuildingObjId,
                AlreadyExistingBuildingId = building.Id,
                CanBuildAs = new(),
                IncludeScript = viewbuildingmodel.ManageModels.Count == 0,
                PrefixForIds = "",
                User = user
            };

            var managemodel = new BuildingManageModel()
            {
                Building = building,
                Name = building.Name,
                Description = building.Description,
                RecipeId = building.RecipeId,
                BuildingId = building.Id,
                createBuildingRequestModel = model
            };

            // only groups/corporations can have building employees
            if (building.Owner.EntityType == EntityType.Group || building.Owner.EntityType == EntityType.Corporation)
            {
                var group = (Group)building.Owner;
                var ownerauthority = group.GetAuthority(user);
                managemodel.GroupRolesForEmployee = new();
                managemodel.GroupRolesForEmployee.Add(new("None", "0"));
                managemodel.GroupRolesForEmployee.AddRange(group.Roles.Where(x => x.Authority < ownerauthority).Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == building.EmployeeGroupRoleId)));
            }

            viewbuildingmodel.ManageModels.Add(managemodel);
        }

        return View(viewbuildingmodel);
    }

    [UserRequired]
    public IActionResult Edit(long id) {
        var user = HttpContext.GetUser();
        State? state = DBCache.Get<State>(id);
        if (state is null)
            return RedirectBack();

        if (!state.CanEdit(user))
            return RedirectBack("You lack permission to manage this state!");

        return View(state);
    }

    [HttpPost]
    [UserRequired]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(State newstate) {
        State? oldstate = DBCache.Get<State>(newstate.Id);
        if (oldstate is null)
            return Redirect("/");

        var user = HttpContext.GetUser();
        if (!oldstate.CanEdit(user))
            return RedirectBack("You lack permission to edit this state!");

        if (newstate.BasePropertyTax > 1000)
            return RedirectBack("Base Property Tax must be 1,000 or less!");
        if (newstate.PropertyTaxPerSize > 1000)
            return RedirectBack("Property Tax per size must be 1,000 or less!");

        oldstate.Name = newstate.Name;
        oldstate.Description = newstate.Description;
        oldstate.MapColor = newstate.MapColor;
        oldstate.BasePropertyTax = newstate.BasePropertyTax;
        oldstate.PropertyTaxPerSize = newstate.PropertyTaxPerSize;

        StatusMessage = "Successfully saved your changes.";
        return Redirect($"/State/View/{oldstate.Id}");
    }

    [HttpPost("/State/ChangeGovernor/{id}")]
    [ValidateAntiForgeryToken]
    [UserRequired]
    public IActionResult ChangeGovernor(long id, long? GovernorId) {
        State? state = DBCache.Get<State>(id);
        if (state is null)
            return Redirect("/");

        var user = HttpContext.GetUser();
        if (state.District.GovernorId != user.Id)
            return RedirectBack("You must be governor of the district to change the governor of a province!");

        BaseEntity entity = BaseEntity.Find(GovernorId);

        BaseEntity prevgovernor = BaseEntity.Find(state.GovernorId);
        state.GovernorId = GovernorId;

        if (GovernorId is not null)
        {
            if (!state.Group.MembersIds.Contains((long)GovernorId))
                state.Group.MembersIds.Add((long)GovernorId);

            state.Group.AddEntityToRole(DBCache.Get<Group>(100), entity, state.Group.Roles.First(x => x.Name == "Governor"), true);
        }

        if (prevgovernor is not null)
        {
            state.Group.RemoveEntityFromRole(DBCache.Get<Group>(100), prevgovernor, state.Group.Roles.First(x => x.Name == "Governor"), true);
        }

        if (GovernorId is not null)
            return RedirectBack($"Successfully changed the governorship of this province to {BaseEntity.Find(GovernorId).Name}");
        else
            return RedirectBack($"Successfully changed the governorship of this province to none");
    }

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> ManageBuildingRequests(long id, bool? toggleonlyreviewed = false) {
        State? state = DBCache.Get<State>(id);
        if (state is null)
            return Redirect("/");

        var user = HttpContext.GetUser();

        if (!state.CanManageBuildingRequests(user))
            return RedirectBack("You lack permission to manage building requests for this state!");

        List<BuildingRequest> requests = new();
        var idscanmanage = DBCache.GetAll<Province>().Where(x => x.CanManageBuildingRequests(user)).Select(x => x.Id).ToList();
        requests = await _dbctx.BuildingRequests.Where(x => x.Reviewed == toggleonlyreviewed && idscanmanage.Contains(x.ProvinceId)).ToListAsync();

        return View(new ManageBuildingRequestsModel() {
            BuildingRequests = requests,
            State = state
        });
    }
}