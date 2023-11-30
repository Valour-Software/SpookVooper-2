using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Helpers;
using SV2.Models.Users;
using SV2.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SV2.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class UserController : SVController {
    private readonly ILogger<UserController> _logger;
    private readonly VooperDB _dbctx;

    public UserController(ILogger<UserController> logger, VooperDB dbctx)
    {
        _logger = logger;
        _dbctx = dbctx;
    }

    [HttpGet("/User/MyJobOpenings")]
    [UserRequired]
    public async Task<IActionResult> MyJobOpenings()
    {
        var user = HttpContext.GetUser();

        List<long> canbuildasids = new();
        canbuildasids.AddRange(DBCache.GetAll<Group>().Where(x => x.HasPermission(user, GroupPermissions.ManageBuildings)).Select(x => x.Id).ToList());

        var buildings = DBCache.ProducingBuildingsById.Values.Where(x => canbuildasids.Contains(x.OwnerId)).ToList();
        var buildingsids = buildings.Select(x => x.Id).ToList();

        var jobApplications = await _dbctx.JobApplications.Where(x => buildingsids.Contains(x.BuildingId)).ToListAsync();

        var model = new JobOpeningsModel()
        {
            JobOpenings = new()
        };

        foreach (var application in jobApplications)
        {
            var building = buildings.FirstOrDefault(x => x.Id == application.BuildingId);
            if (building.EmployeeId is null && building.EmployeeGroupRoleId is not null && building.EmployeeGroupRoleId != 0)
            {
                model.JobOpenings.Add(new()
                {
                    Building = building,
                    Role = DBCache.Get<GroupRole>(building.EmployeeGroupRoleId),
                    JobApplication = application,
                    User = DBCache.Get<SVUser>(application.UserId)
                });
            }
        }

        model.JobOpenings = model.JobOpenings.OrderByDescending(x => x.Role.Salary).ToList();

        return View(model);
    }

    [HttpGet("/User/JobOpenings")]
    [UserRequired]
    public async Task<IActionResult> JobOpenings()
    {
        var user = HttpContext.GetUser();
        var usersJobApplications = await _dbctx.JobApplications.Where(x => x.UserId == user.Id).ToListAsync();

        var model = new JobOpeningsModel()
        {
            JobOpenings = new()
        };

        foreach (var building in DBCache.GetAllProducingBuildings())
        {
            if (building.EmployeeId is null && building.EmployeeGroupRoleId is not null && building.EmployeeGroupRoleId != 0) {
                model.JobOpenings.Add(new()
                {
                    Building = building,
                    Role = DBCache.Get<GroupRole>(building.EmployeeGroupRoleId),
                    JobApplication = usersJobApplications.FirstOrDefault(x => x.BuildingId == building.Id)
                });
            }
        }

        model.JobOpenings = model.JobOpenings.OrderByDescending(x => x.Role.Salary).ToList();

        return View(model);
    }

    [HttpGet("/User/ApplyToJob")]
    [UserRequired]
    public async Task<IActionResult> ApplyToJob(long buildingid)
    {
        var user = HttpContext.GetUser();

        var building = DBCache.ProducingBuildingsById[buildingid];
        var application = new JobApplication()
        {
            UserId = user.Id,
            Id = IdManagers.GeneralIdGenerator.Generate(),
            BuildingId = buildingid,
            Accepted = false,
            Reviewed = false
        };

        _dbctx.JobApplications.Add(application);
        await _dbctx.SaveChangesAsync();

        return RedirectBack("Successfully applied to the job.");
    }

    [HttpGet("/User/UnApplyToJob")]
    [UserRequired]
    public async Task<IActionResult> StartWorking(long applicationid)
    {
        var user = HttpContext.GetUser();

        var application = await _dbctx.JobApplications.FindAsync(applicationid);
        _dbctx.JobApplications.Remove(application);
        await _dbctx.SaveChangesAsync();

        return RedirectBack("Successfully unapplied.");
    }

    [HttpGet("/User/RemoveJobApplication")]
    [UserRequired]
    public async Task<IActionResult> RemoveJobApplication(long applicationid)
    {
        var user = HttpContext.GetUser();

        var application = await _dbctx.JobApplications.FindAsync(applicationid);
        _dbctx.JobApplications.Remove(application);
        await _dbctx.SaveChangesAsync();

        return RedirectBack("Successfully removed job application.");
    }

    [HttpGet("/User/AcceptJobOffer")]
    [UserRequired]
    public async Task<IActionResult> AcceptJobOffer(long applicationid)
    {
        var user = HttpContext.GetUser();

        var application = await _dbctx.JobApplications.FindAsync(applicationid);
        var building = DBCache.ProducingBuildingsById[application.BuildingId];
        if (!application.Accepted)
            return RedirectBack("You can only start working if your application has been accepted!");
        if (user.GetNumberOfJobSlotsFilled() >= 2)
            return RedirectBack("You have used your two (2) available job slots!");

        building.EmployeeId = user.Id;

        var group = (Group)building.Owner;
        var role = DBCache.Get<GroupRole>(building.EmployeeGroupRoleId);

        if (!group.MembersIds.Contains(user.Id))
            group.MembersIds.Add(user.Id);

        // yes this will mean that a user might be added two times to a role
        role.MembersIds.Add(user.Id);

        _dbctx.JobApplications.RemoveRange(await _dbctx.JobApplications.Where(x => x.BuildingId == building.Id).ToListAsync());

        await _dbctx.SaveChangesAsync();

        return RedirectBack($"Successfully accepted job offer, you are now working at {building.Name}.");
    }

    [HttpGet("/User/RejectJobOffer")]
    [UserRequired]
    public async Task<IActionResult> RejectJobOffer(long applicationid)
    {
        var user = HttpContext.GetUser();

        var application = await _dbctx.JobApplications.FindAsync(applicationid);
        _dbctx.JobApplications.Remove(application);
        await _dbctx.SaveChangesAsync();

        return RedirectBack($"Successfully rejected job offer.");
    }

    [HttpGet("/User/AcceptUsersJobApplication")]
    [UserRequired]
    public async Task<IActionResult> AcceptUsersJobApplication(long applicationid)
    {
        var user = HttpContext.GetUser();

        var application = await _dbctx.JobApplications.FindAsync(applicationid);

        application.Reviewed = true;
        application.Accepted = true;

        await _dbctx.SaveChangesAsync();

        return RedirectBack($"Successfully accepted job application.");
    }

    [HttpGet("/User/RejectUsersJobApplication")]
    [UserRequired]
    public async Task<IActionResult> RejectUsersJobApplication(long applicationid)
    {
        var user = HttpContext.GetUser();

        var application = await _dbctx.JobApplications.FindAsync(applicationid);

        application.Reviewed = true;
        application.Accepted = false;

        await _dbctx.SaveChangesAsync();

        return RedirectBack($"Successfully rejected job application.");
    }

    [HttpGet("/User/Info/{id}")]
    public async Task<IActionResult> Info(long? id)
    {
        if (id is null)
            return View(null);

        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.Id == id);

        return View(user);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}