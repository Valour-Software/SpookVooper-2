using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using SV2.Database.Models.Permissions;
using System.Diagnostics;
using SV2.Helpers;
using SV2.Extensions;
using Microsoft.AspNetCore.Identity;
using SV2.Models.Groups;
using Valour.Api.Models;

namespace SV2.Controllers;

public class GroupController : SVController
{
    private readonly ILogger<GroupController> _logger;
    
    [TempData]
    public string StatusMessage { get; set; }

    public GroupController(ILogger<GroupController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult View(long id)
    {
        Group? group = Group.Find(id);
        return View(group);
    }

    public IActionResult Create()
    {
        SVUser? user = UserManager.GetUser(HttpContext);

        if (user is null) 
        {
            return Redirect("/account/login");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Group model)
    {
        SVUser? user = UserManager.GetUser(HttpContext);
        if (user is null) 
            return Redirect("/account/login");

        using var dbctx = VooperDB.DbFactory.CreateDbContext();

        model.Name = model.Name.Trim();

        if (DBCache.GetAll<Group>().Any(x => x.Name == model.Name)) {
            StatusMessage = $"Error: Name {model.Name} is already taken!";
            return Redirect($"/group/create");
        }

        Group group = new Group(model.Name, user.Id);
        group.Description = model.Description;
        group.GroupType = model.GroupType;
        group.DistrictId = model.DistrictId;
        group.ImageUrl = model.ImageUrl;
        group.OwnerId = user.Id;

        DBCache.Put(group.Id, group);
        dbctx.Groups.Add(group);
        await dbctx.SaveChangesAsync();

        return Redirect($"/group/view/{group.Id}");
    }

    public IActionResult Edit(long id)
    {
        Group? group = Group.Find(id);
        return View(group);
    }

    public async Task<IActionResult> MyGroups()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Group model)
    {
        //if (!ModelState.IsValid)
        //{
        //    return View(model);
        //}

        SVUser? user = UserManager.GetUser(HttpContext);

        if (user is null) 
        {
            return Redirect("/account/login");
        }

        Group prevgroup = Group.Find(model.Id)!;

        if (prevgroup == null)
        {
            StatusMessage = $"Error: Group {model.Name} does not exist!";
            return RedirectToAction("Index", controllerName: "Home");
        }

        if (model.Name != prevgroup.Name) 
        {
            if (DBCache.GetAll<Group>().Any(x => x.Name == model.Name)) {
                StatusMessage = $"Error: Name {model.Name} is already taken!";
                return Redirect($"/group/edit/{prevgroup.Id}");
            }
        }

        if (!prevgroup.HasPermission(user, GroupPermissions.Edit))
        {
            StatusMessage = $"Error: You lack permission to edit this Group!";
            return Redirect($"/group/edit/{prevgroup.Id}");
        }

        if (prevgroup.GroupType != model.GroupType)
        {
            StatusMessage = $"Error: Group Type cannot be changed!";
            return Redirect($"/group/edit/{prevgroup.Id}");
        }

        if (prevgroup.OwnerId == user.Id)
        {
            prevgroup.Name = model.Name;
            prevgroup.ImageUrl = model.ImageUrl;
            prevgroup.Open = model.Open;
            prevgroup.DistrictId = model.DistrictId;
            prevgroup.Description = model.Description;
        }

        StatusMessage = $"Successfully edited {prevgroup.Name}!";

        return Redirect($"/group/view/{prevgroup.Id}");
    }

    [UserRequired]
    public IActionResult ViewMemberRoles(long groupid, long targetid)
    {
        var group = DBCache.Get<Group>(groupid);
        if (group == null) return RedirectBack($"Error: Group does not exist!");
        var target = DBCache.FindEntity(targetid);

        return View(new ViewMemberRolesModel() { Group = group, Target = target });
    }

    [UserRequired]
    public IActionResult AddEntityToRole(long groupid, long targetid, long roleid)
    {
        var group = DBCache.Get<Group>(groupid);
        if (group == null) return RedirectBack($"Error: Group does not exist!");
        
        var user = HttpContext.GetUser();
        var role = DBCache.Get<GroupRole>(roleid);
        var target = DBCache.FindEntity(targetid);

        var result = group.AddEntityToRole(user, target, role);
        return RedirectBack(result.Info);
    }

    [UserRequired]
    public IActionResult RemoveEntityFromRole(long groupid, long targetid, long roleid)
    {
        var group = DBCache.Get<Group>(groupid);
        if (group == null) return RedirectBack($"Error: Group does not exist!");

        var user = HttpContext.GetUser();
        var role = DBCache.Get<GroupRole>(roleid);
        var target = DBCache.FindEntity(targetid);

        var result = group.RemoveEntityFromRole(user, target, role);
        return RedirectBack(result.Info);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}