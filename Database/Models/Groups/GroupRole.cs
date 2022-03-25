using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Entities;
using SpookVooper_2.Database.Models.Permissions;

namespace SpookVooper_2.Database.Models.Groups;

public class GroupRole
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    // this role's permission value
    public ulong PermissionValue { get; set; }

    // Hexcode for role color (ex: #ffffff)
    public string Color { get; set; }

    // The group this role belongs to
    public string Group_Id { get; set; }

    // Salary for role, paid every hour
    public decimal Salary { get; set; }
    public int Authority { get; set; }
}