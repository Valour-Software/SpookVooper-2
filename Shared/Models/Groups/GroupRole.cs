using Shared.Models.Entities;

namespace Shared.Models.Groups;

public class GroupRole
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long PermissionValue { get; set; }
    public List<long> MembersIds { get; set; }
    public string Color { get; set; }
    public long GroupId { get; set; }
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

    public string GetPermissions()
    {
        string output = "";
        bool First = true;
        foreach (GroupPermission perm in GroupPermissions.Permissions)
        {
            if ((perm.Value & PermissionValue) == perm.Value)
            {
                if (!First)
                    output += ", ";
                output += perm.Name;
                First = false;
            }
        }
        return output;
    }

    public bool HasPermission(GroupPermission permission)
    {
        return Permission.HasPermission(PermissionValue, permission);
    }

    public async ValueTask<Group> GetGroupAsync()
    {
        return await Group.FindAsync(GroupId);
    }

    public async ValueTask<List<BaseEntity>> GetMembersAsync()
    {
        var members = new List<BaseEntity>();
        foreach (var id in MembersIds)
        {
            members.Add(await BaseEntity.FindAsync(id));
        }

        return members;
    }


}