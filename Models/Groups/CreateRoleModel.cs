using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SV2.Models.Groups;

public class CreateRoleModel
{
    // The ID of the role
    [Key]
    public long RoleId { get; set; }

    // The name of the role
    [MaxLength(64, ErrorMessage = "Name should be under 64 characters.")]
    [RegularExpression("^[a-zA-Z0-9, ]*$", ErrorMessage = "Please use only letters, numbers, and commas.")]
    [Display(Name = "Name", Description = "Use the name 'Default' for the permissions given to those without any roles! (Only make one default role!!!)")]
    [Required]
    public string Name { get; set; }

    // Things this role is allowed to do
    public long PermissionValue { get; set; }

    // Hexcode for role color
    [MaxLength(6, ErrorMessage = "Color should be a hex code (ex: #ffffff)")]
    [Display(Name = "Color", Description = "The hex color code for the role. #??????")]
    public string Color { get; set; }

    // ids of members of this role
    public long MembersIds { get; set; }

    public List<BaseEntity> Members { get; set; }

    // The group this role belongs to
    public long GroupId { get; set; }

    [Display(Name = "Salary", Description = "This will be paid to this role every hour.")]
    public decimal Salary { get; set; }

    // Weight of the role
    [Required]
    [RegularExpression("^[0-9]*$", ErrorMessage = "Numbers only!")]
    [Display(Name = "Authority", Description = "The 'Power' of a role. A role with less power cannot be banned, kicked, or give themselves a higher role.")]
    public int Authority { get; set; }

    [Display(Name = "Create Role", Description = "The ability to create a role. This is the most powerful and dangerous permission, " +
        "as there are no limits to the roles created. This also lets you set the default role.")]
    public bool CreateRole { get; set; }

    [Display(Name = "Add To Role", Description = "The ability to add a user to a role.")]
    public bool AddRole { get; set; }

    [Display(Name = "Remove From Role", Description = "The ability to remove a user from a role.")]
    public bool RemoveRole { get; set; }

    [Display(Name = "Invite", Description = "The ability to invite a user to the group.")]
    public bool Invite { get; set; }

    [Display(Name = "Uninvite", Description = "The ability to remove invites.")]
    public bool Uninvite { get; set; }

    [Display(Name = "Kick", Description = "The ability to kick a user from the group.")]
    public bool Kick { get; set; }

    [Display(Name = "Ban", Description = "The ability to ban a user from the group, and to unban users.")]
    public bool Ban { get; set; }

    [Display(Name = "Edit", Description = "The ability to get onto the 'Edit' page of a group. This doesn't mean you get " +
        "permissions for everything in Edit!")]
    public bool Edit { get; set; }

    [Display(Name = "Description", Description = "The ability to set the description of the group.")]
    public bool Description { get; set; }

    [Display(Name = "Post", Description = "The ability to post in a group's category.")]
    public bool Post { get; set; }

    [Display(Name = "Economy", Description = "The ability to use group finances.")]
    public bool Eco { get; set; }

    [Display(Name = "Plot Management", Description = "The ability to purchase and modify group plots [Nerdcraft].")]
    public bool Plots { get; set; }

    [Display(Name = "News", Description = "The ability to post news stories.")]
    public bool News { get; set; }

    public static CreateRoleModel FromExisting(GroupRole role)
    {
        CreateRoleModel model = new CreateRoleModel()
        {
            Color = role.Color,
            GroupId = role.GroupId,
            RoleId = role.RoleId,
            Weight = role.Weight,
            Name = role.Name,
            Salary = role.Salary
        };

        model.CreateRole = role.Permissions.Contains("createrole|");
        model.AddRole = role.Permissions.Contains("addrole|");
        model.RemoveRole = role.Permissions.Contains("removerole|");
        model.Invite = role.Permissions.Contains("addinvite|");
        model.Uninvite = role.Permissions.Contains("uninvite|");
        model.Kick = role.Permissions.Contains("kick|");
        model.Ban = role.Permissions.Contains("ban|");
        model.Edit = role.Permissions.Contains("edit|");
        model.Description = role.Permissions.Contains("description|");
        model.Post = role.Permissions.Contains("post|");
        model.Eco = role.Permissions.Contains("eco|");
        model.Plots = role.Permissions.Contains("plot|");
        model.Plots = role.Permissions.Contains("news|");

        return model;
    }
}
}
