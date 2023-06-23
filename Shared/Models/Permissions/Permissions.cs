namespace Shared.Models.Permissions;

/// <summary>
/// This class contains all group permissions and helper methods for working
/// with them.
/// </summary>
public static class GroupPermissions
{
    /// <summary>
    /// Contains every group permission
    /// </summary>
    public static GroupPermission[] Permissions;

    static GroupPermissions()
    {
        Permissions = new GroupPermission[]
        {
                FullControl,
                CreateRole,
                DeleteRole,
                AddMembersToRoles,
                RemoveMembersFromRoles,
                ManageInvites,
                ManageMembership,
                Post,
                Eco,
                Edit,
                News,
                ManageBuildingRequests,
                ManageProvinces,
                Build,
                ManageBuildings,
                Resources,
                Recipes
        };
    }

    // Use shared full control definition
    public static readonly GroupPermission FullControl = new GroupPermission(Permission.FULL_CONTROL, "Full Control", "Control every part of this group.");

    // Every subsequent permission has double the value (the next bit)
    // An update should NEVER change the order or value of old permissions
    // As that would be a massive security issue
    public static readonly GroupPermission CreateRole = new GroupPermission(0x01, "Create Roles", "Allows members to create roles.");
    public static readonly GroupPermission DeleteRole = new GroupPermission(0x02, "Delete Roles", "Allows members to delete roles.");
    public static readonly GroupPermission AddMembersToRoles = new GroupPermission(0x04, "Add To Role", "Allows members to add entities to roles.");
    public static readonly GroupPermission RemoveMembersFromRoles = new GroupPermission(0x08, "Remove From Role", "Allows members to remove entities from roles.");
    public static readonly GroupPermission ManageInvites = new GroupPermission(0x10, "Manage Invites", "Allows members to manage invites.");
    public static readonly GroupPermission ManageMembership = new GroupPermission(0x20, "Manage Membership", "Allows members to kick or ban users from the group.");
    public static readonly GroupPermission Post = new GroupPermission(0x40, "Post", "Allows members to post as this group. For example to make group forum posts or post new articles.");
    public static readonly GroupPermission Eco = new GroupPermission(0x80, "Eco", "Allows members to send transactions, trade stocks, etc as this group.");
    public static readonly GroupPermission Edit = new GroupPermission(0x100, "Edit", "Allows members to edit details about this group.");
    public static readonly GroupPermission News = new GroupPermission(0x200, "News", "Allows members to post news under this group.");
    public static readonly GroupPermission ManageBuildingRequests = new GroupPermission(0x400, "Manage Building Requests", "Allows members to accept or deny building requests on provinces that this group has governorship over.");
    public static readonly GroupPermission ManageProvinces = new GroupPermission(0x800, "Manage Provinces", "Allows members to edit provinces that this group has governorship over.");
    public static readonly GroupPermission Build = new GroupPermission(0x1000, "Build", "Allows members to submit building requests as this group.");
    public static readonly GroupPermission ManageBuildings = new GroupPermission(0x2000, "Manage Buildings", "Allows members to manage building owned by this group.");
    public static readonly GroupPermission Resources = new GroupPermission(0x4000, "Resources", "Allows members to send resource trades as this group.");
    public static readonly GroupPermission Recipes = new GroupPermission(0x8000, "Recipes", "Stuff");
}

public class GroupPermission : Permission
{
    public override string ReadableName => "Group";
    public GroupPermission(long value, string name, string description) : base(value, name, description)
    {
    }
}

/// <summary>
/// Permissions are basic flags used to denote if actions are allowed
/// to be taken on one's behalf
/// </summary>
public class Permission
{
    /// <summary>
    /// Permission node to have complete control
    /// </summary>
    public const long FULL_CONTROL = long.MaxValue;

    /// <summary>
    /// The name of this permission
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of this permission
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The value of this permission
    /// </summary>
    public long Value { get; set; }

    public virtual string ReadableName => "Base";

    /// <summary>
    /// Initializes the permission
    /// </summary>
    public Permission(long value, string name, string description)
    {
        this.Name = name;
        this.Description = description;
        this.Value = value;
    }

    /// <summary>
    /// Returns whether the given code includes the given permission
    /// </summary>
    public static bool HasPermission(long code, Permission permission)
    {
        // Case if full control is granted
        if (code == FULL_CONTROL) return true;

        // Otherwise check for specific permission
        return (code & permission.Value) == permission.Value;
    }

    /// <summary>
    /// Creates and returns a permission code from given permissions 
    /// </summary>
    public static long CreateCode(params Permission[] permissions)
    {
        long code = 0x00;

        foreach (Permission permission in permissions)
        {
            if (permission != null)
            {
                code |= permission.Value;
            }
        }

        return code;
    }
}

public enum PermissionState
{
    Undefined, True, False
}

/// <summary>
/// Permission codes use two ulongs to represent
/// three possible states for every permission
/// </summary>
public struct PermissionCode
{
    // Just for reference,
    // If the mask bit is 0, then it is always undefined
    // If the mask but is 1, then if the code bit is 1 it is true. Otherwise it is false.
    // This basically compresses 64 booleans (64 bytes) into 2 ulongs (16 bytes)

    public long Code { get; set; }
    public long Mask { get; set; }

    public PermissionCode(long code, long mask)
    {
        this.Code = code;
        this.Mask = mask;
    }

    public PermissionState GetState(Permission permission)
    {
        if ((Mask & permission.Value) != permission.Value)
        {
            return PermissionState.Undefined;
        }

        if ((Code & permission.Value) != permission.Value)
        {
            return PermissionState.False;
        }

        return PermissionState.True;
    }
}