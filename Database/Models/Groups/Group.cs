using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;

namespace SV2.Database.Models.Groups;

public enum GroupType
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

public class Group : IHasOwner, IEntity
{
    [Key]
    [EntityId]
    public string Id { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(2048)]
    public string? Description { get; set; }

    [VarChar(512)]
    public string? Image_Url { get; set; }
    
    [EntityId]
    public string? DistrictId { get; set;}
    public decimal Credits { get; set;}
    public decimal CreditsYesterday { get; set;}
    
    [JsonIgnore]
    [VarChar(36)]
    public string Api_Key { get; set; }
    public GroupType GroupType { get; set; }
    // will be use the PostgreSQL Array datatype
    public List<GroupFlag> Flags { get; set; }
    // if the group is open to the public
    public bool Open { get; set; }
    
    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]

    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    public Group(string name, string ownerId)
    {
        Id = "g-"+Guid.NewGuid().ToString();
        Name = name;
        Api_Key = Guid.NewGuid().ToString();
        Credits = 0.0m;
        CreditsYesterday = 0.0m;
        OwnerId = ownerId;
        Open = false;
        Flags = new();
        GroupType = GroupType.Company;
    }

    public GroupRole GetHighestRole(IEntity user) 
    {
        GroupRole role = DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(user.Id)).OrderByDescending(x => x.Authority).First();
        return role;
    }

    public GroupRole GetHighestRoleWithPermission(IEntity user, GroupPermission permission) 
    {
        GroupRole role = DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(user.Id) && HasPermission(user, permission)).OrderByDescending(x => x.Authority).First();
        return role;
    }

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        if (apikey == Api_Key) {
            return true;
        }
        
        // add oauth key handling
        return false;
        
    }

    public bool HasPermission(IEntity entity, GroupPermission permission)
    {
        if (entity.Id == OwnerId) {
            return true;
        }

        foreach(GroupRole role in DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(entity.Id)).OrderByDescending(x => x.Authority))
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

    public static async Task<Group?> FindAsync(string Id)
    {
        if (DBCache.Contains<Group>(Id)) {
            return DBCache.Get<Group>(Id);
        }
        Group? group = await VooperDB.Instance.Groups.FindAsync(Id);
        await DBCache.Put<Group>(Id, group);
        return group;
    }
}