using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace SV2.Database.Models.Groups;

public enum GroupTypes
{
    Company,
    // a corporation is a company that is listed on SVSE or a company on a private stock exchange that the CFV has determined is a corporation
    Corporation,
    NonProfit,
    PoliticalParty,
    District
}

public enum GroupFlag
{
    // is only given by the CFV
    Charity,
    // is only given by the MOJ
    News
}

public class Group : BaseEntity, IHasOwner
{
    public GroupTypes GroupType { get; set; }

    // use the PostgreSQL Array datatype
    public List<GroupFlag> Flags { get; set; }

    // if the group is open to the public
    public bool Open { get; set; }

    public List<long> MembersIds { get; set; }

    public override EntityType EntityType
    {
        get
        {
            if (GroupType == GroupTypes.Corporation)
                return EntityType.Corporation;
            return EntityType.Group;
        }
    }

    public bool IsInGroup(SVUser user)
    {
        return MembersIds.Contains(user.Id);
    }

    public IEnumerable<SVUser> GetMembers()
    {
        return MembersIds.Select(x => SVUser.Find(x));
    }

    public long OwnerId { get; set; }

    [NotMapped]

    public BaseEntity Owner
    {
        get
        {
            return BaseEntity.Find(OwnerId)!;
        }
    }

    public Group()
    {

    }

    public Group(string name, long ownerId)
    {
        Id = IdManagers.GroupIdGenerator.Generate();
        Name = name;
        ApiKey = Guid.NewGuid().ToString();
        Credits = 0.0m;
        CreditSnapshots = new();
        OwnerId = ownerId;
        Open = false;
        Flags = new();
        GroupType = GroupTypes.Company;
        MembersIds = new() { OwnerId };
    }

    public GroupRole? GetHighestRole(BaseEntity user)
    {
        GroupRole? role = DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(user.Id)).OrderByDescending(x => x.Authority).FirstOrDefault();
        if (role is null)
        {
            return GroupRole.Default;
        }
        return role;
    }

    public GroupRole GetHighestRoleWithPermission(BaseEntity user, GroupPermission permission)
    {
        GroupRole role = DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(user.Id) && HasPermission(user, permission)).OrderByDescending(x => x.Authority).First();
        return role;
    }

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        if (apikey == ApiKey)
        {
            return true;
        }

        // add oauth key handling
        return false;

    }

    public bool HasPermission(BaseEntity entity, GroupPermission permission)
    {
        if (entity.Id == OwnerId)
        {
            return true;
        }

        foreach (GroupRole role in DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(entity.Id)).OrderByDescending(x => x.Authority))
        {
            PermissionCode code = new PermissionCode(role.PermissionValue, permission.Value);
            PermissionState state = code.GetState(permission);

            if (state == PermissionState.Undefined)
            {
                continue;
            }
            else if (state == PermissionState.True)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;

    }

    public async Task<IEnumerable<Group>> GetOwnedGroupsAsync()
    {
        List<Group> groups = new List<Group>();

        using var dbctx = VooperDB.DbFactory.CreateDbContext();

        var topGroups = await dbctx.Groups.Where(x => x.OwnerId == Id).ToListAsync();

        foreach (Group group in topGroups)
        {
            groups.Add(group);
            groups.AddRange(await group.GetOwnedGroupsAsync());
        }

        return groups;
    }

    public static Group? Find(long Id)
    {
        return DBCache.Get<Group>(Id);
    }
}