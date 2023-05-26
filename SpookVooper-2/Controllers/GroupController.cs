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
using Microsoft.AspNetCore.Authorization;
using SV2.Models.Manage;

namespace SV2.Controllers;

public class GroupController : SVController
{
    private readonly ILogger<GroupController> _logger;

    private readonly VooperDB _dbctx;

    public GroupController(ILogger<GroupController> logger, VooperDB dbctx)
    {
        _logger = logger;
        _dbctx = dbctx;
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
    public async Task<IActionResult> Create(CreateGroupModel model)
    {
        SVUser? user = UserManager.GetUser(HttpContext);
        if (user is null) 
            return Redirect("/account/login");

        model.Name = model.Name.Trim();

        if (DBCache.GetAll<Group>().Any(x => x.Name == model.Name))
            return RedirectBack($"Error: Name {model.Name} is already taken!");
        
        if (DBCache.GetAll<Group>().Count(x => x.OwnerId == user.Id) > 15)
            return RedirectBack($"You can not own more than 15 groups!");

        Group group = new Group(model.Name, user.Id) {
            Description = model.Description,
            GroupType = model.GroupType,
            DistrictId = model.DistrictId,
            ImageUrl = model.ImageUrl,
            OwnerId = user.Id
        };

        DBCache.AddNew(group.Id, group);

        return Redirect($"/group/view/{group.Id}");
    }

    [UserRequired]
    public IActionResult Edit(long id)
    {
        Group? group = Group.Find(id);
        var user = HttpContext.GetUser();
        if (!group.HasPermission(user, GroupPermissions.Edit))
            return RedirectBack("You lack permission to edit this group!");
        return View(new EditGroupModel() {
            Group = group,
            Name = group.Name,
            Description = group.Description,
            Open = group.Open,
            Id = group.Id,
            DistrictId = group.DistrictId,
            ImageUrl = group.ImageUrl,
            GroupType = group.GroupType
        });
    }

    [UserRequired]
    public IActionResult AcceptInvite(long groupid, long inviteeid) 
    {
        var user = HttpContext.GetUser();
        var group = DBCache.Get<Group>(groupid);
        var invitee = BaseEntity.Find(inviteeid);
        if (invitee.EntityType == EntityType.User && invitee.Id != user.Id)
            return RedirectBack("You can not accept an invite for someone else!");
        if ((invitee.EntityType == EntityType.Group || invitee.EntityType == EntityType.Corporation) && !((Group)invitee).IsOwner(user))
            return RedirectBack("You can not accept an invite for a group you don't own!");
        if (!group.Invited.Contains(inviteeid))
            return RedirectBack($"{invitee.Name} has not been invited to this group!");
        group.Invited.Remove(inviteeid);
        group.MembersIds.Add(invitee.Id);
        return RedirectBack($"{invitee.Name} successfully joined {group.Name}");
    }

    [UserRequired]
    public IActionResult RejectInvite(long groupid, long inviteeid) {
        var user = HttpContext.GetUser();
        var group = DBCache.Get<Group>(groupid);
        var invitee = BaseEntity.Find(inviteeid);
        if (invitee.EntityType == EntityType.User && invitee.Id != user.Id)
            return RedirectBack("You can not reject an invite for someone else!");
        if ((invitee.EntityType == EntityType.Group || invitee.EntityType == EntityType.Corporation) && !((Group)invitee).IsOwner(user))
            return RedirectBack("You can not reject an invite for a group you don't own!");
        if (!group.Invited.Contains(inviteeid))
            return RedirectBack($"{invitee.Name} has not been invited to this group!");
        group.Invited.Remove(inviteeid);
        return RedirectBack($"{invitee.Name} successfully rejected invite to {group.Name}");
    }

    [UserRequired]
    public IActionResult ViewInvited() 
    {
        var user = HttpContext.GetUser();

        List<long> canacceptinvitesids = new() { user.Id };
        canacceptinvitesids.AddRange(
            DBCache.GetAll<Group>().Where(x => x.IsOwner(user))
            .Select(x => x.Id).ToList());

        List<InvitedModel> invitedmodels = new();
        foreach (var group in DBCache.GetAll<Group>().Where(x => x.Invited.Any(id => canacceptinvitesids.Contains(id))))
        {
            foreach (var entityid in canacceptinvitesids) 
            {
                if (group.Invited.Contains(entityid)) {
                    invitedmodels.Add(new() {
                        InvitedEntity = BaseEntity.Find(entityid),
                        InvitedTo = group
                    });
                }
            }
        }    
        return View(invitedmodels);
    }

    [HttpGet]
    [UserRequired]
    public IActionResult Invite(long id) {
        Group group = Group.Find(id);
        if (group is null) return RedirectBack("group is null!");

        var user = HttpContext.GetUser();
        if (!group.HasPermission(user, GroupPermissions.ManageInvites))
            return RedirectBack("You lack permission to invite entities!");

        return View(group);
    }

    [HttpPost("/Group/{groupid}/Invite")]
    [ValidateAntiForgeryToken]
    [UserRequired]
    public IActionResult Invite(long groupid, long EntityId)
    {
        Group group = Group.Find(groupid);
        if (group is null) return RedirectBack("group is null!");

        var user = HttpContext.GetUser();
        if (!group.HasPermission(user, GroupPermissions.ManageInvites))
            return RedirectBack("You lack permission to invite entities!");

        var entitytobeinvited = BaseEntity.Find(EntityId);
        if (entitytobeinvited is null) return RedirectBack("Entity to invite could not be found!");
        if (group.MembersIds.Contains(entitytobeinvited.Id)) return RedirectBack("Entity is already a member of this group!");
        if (group.Invited.Contains(entitytobeinvited.Id)) return RedirectBack("Entity has already been invited!");
        group.Invited.Add(entitytobeinvited.Id);

        return RedirectBack($"Successfully invited {entitytobeinvited.Name}");
    }

    public async Task<IActionResult> MyGroups()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [UserRequired]
    public IActionResult Edit(EditGroupModel model)
    {
        //if (!ModelState.IsValid)
        //{
        //    return View(model);
        //}

        SVUser user = HttpContext.GetUser();

        Group prevgroup = Group.Find(model.Id)!;

        if (prevgroup == null)
        {
            StatusMessage = $"Error: Group {model.Name} does not exist!";
            return RedirectToAction("Index", controllerName: "Home");
        }

        if (model.Name != prevgroup.Name)
        {
            if (DBCache.GetAll<Group>().Any(x => x.Name == model.Name))
            {
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

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> CreateRole(long groupid, long roleid)
    {
        Group group = Group.Find(groupid);

        var user = HttpContext.GetUser();

        if (!group.HasPermission(user, GroupPermissions.CreateRole))
            return RedirectBack("You lack the CreateRole permission!");

        GroupRole role = DBCache.Get<GroupRole>(roleid);

        CreateRoleModel model;

        if (role is null)
        {
            model = new CreateRoleModel()
            {
                GroupId = group.Id
            };
        }
        else
        {
            model = CreateRoleModel.FromExisting(role);
        }

        return View(model);
    }

    [HttpPost]
    [UserRequired]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateRoleModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        Group group = Group.Find(model.GroupId);
        var user = HttpContext.GetUser();

        if (!group.HasPermission(user, GroupPermissions.CreateRole))
            return RedirectBack("You lack the CreateRole permission!");

        long permcode = 0;

        if (model.CreateRole) { permcode |= GroupPermissions.CreateRole.Value; }
        if (model.DeleteRole) { permcode |= GroupPermissions.DeleteRole.Value; }
        if (model.RemoveRole) { permcode |= GroupPermissions.RemoveMembersFromRoles.Value; }
        if (model.AddRole) { permcode |= GroupPermissions.AddMembersToRoles.Value; }
        if (model.ManageInvites) { permcode |= GroupPermissions.ManageInvites.Value; }
        if (model.ManageMembership) { permcode |= GroupPermissions.ManageMembership.Value; }
        if (model.Edit) { permcode |= GroupPermissions.Edit.Value; }
        if (model.Post) { permcode |= GroupPermissions.Post.Value; }
        if (model.Eco) { permcode |= GroupPermissions.Eco.Value; }
        if (model.News) { permcode |= GroupPermissions.News.Value; }
        if (model.ManageProvinces) { permcode |= GroupPermissions.ManageProvinces.Value; }
        if (model.ManageBuildingRequests) { permcode |= GroupPermissions.ManageBuildingRequests.Value; }
        if (model.Build) { permcode |= GroupPermissions.Build.Value; }
        if (model.Resources) { permcode |= GroupPermissions.Resources.Value; }

        if (model.RoleId == 0)
            model.RoleId = IdManagers.GeneralIdGenerator.Generate();

        GroupRole role = DBCache.Get<GroupRole>(model.RoleId);

        if (role is null)
        {
            role = new GroupRole()
            {
                Name = model.Name,
                Color = model.Color,
                GroupId = model.GroupId,
                PermissionValue = permcode,
                Id = model.RoleId,
                Authority = model.Authority,
                Salary = model.Salary,
                MembersIds = new()
            };
            DBCache.Put(role.Id, role);
            DBCache.dbctx.GroupRoles.Add(role);
            //await _dbctx.SaveChangesAsync();
        }

        else
        {
            role.Name = model.Name;
            role.Color = model.Color;
            role.Salary = model.Salary;
            role.PermissionValue = permcode;
            role.Authority = model.Authority;
        }

        StatusMessage = $"Successfully created role {role.Name}";
        return Redirect($"/group/edit/{group.Id}");
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