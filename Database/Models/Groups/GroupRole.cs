using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Users;
using SV2.Database.Models.Permissions;

namespace SV2.Database.Models.Groups;

public class GroupRole
{
    [Key]
    public long Id {get; set; }

    [VarChar(64)]
    public string Name { get; set; }
    
    // this role's permission value
    public ulong PermissionValue { get; set; }

    public List<long> Members { get; set; }

    // Hexcode for role color (ex: #ffffff)
    public string Color { get; set; }

    // The group this role belongs to
    public long GroupId { get; set; }

    [NotMapped]
    public Group Group { 
        get {
            return DBCache.Get<Group>(GroupId)!;
        }
    }
    
    // Salary for role, paid every hour
    public decimal Salary { get; set; }
    public int Authority { get; set; }

    public static GroupRole Default = new GroupRole()
    {
        Color = "",
        GroupId = 0,
        Name = "Default Role",
        Authority = int.MinValue,
        PermissionValue = 0
    };

    public GroupRole()
    {

    }

    public IEnumerable<User> GetMembers()
    {
        return DBCache.GetAll<User>().Where(x => Members.Contains(x.Id));
    }

    public List<String> GetPermissions()
    {
        List<String> strings = new();
        foreach(GroupPermission perm in Enum.GetValues(typeof(GroupPermissions)))
        {
            if ((perm.Value & PermissionValue) == perm.Value) {
                strings.Add(perm.Name);
            }
        }
        return strings;
    }

    public GroupRole(string name, long groupid, decimal salary, int authority)
    {
        Id = IdManagers.GeneralIdGenerator.Generate();
        Name = name;
        PermissionValue = 0;
        Members = new();
        Color = "ffffff";
        GroupId = groupid;
        Salary = salary;
        Authority = authority;
    }

    public bool HasPermission(GroupPermission permission)
    {
        return Permission.HasPermission(PermissionValue, permission);
    }
}