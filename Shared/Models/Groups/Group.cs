namespace Shared.Models.Groups;

public enum GroupTypes
{
    Company = 0,
    // a corporation is a company that is listed on SVSE or a company on a private stock exchange that the CFV has determined is a corporation
    Corporation = 1,
    NonProfit = 2,
    PoliticalParty = 3,
    District = 4,
    State = 5
}

public enum ReadableGroupTypes {
    Company = 0,
    NonProfit = 2,
    PoliticalParty = 3
}

public enum GroupFlag
{
    // is only given by the CFV
    Charity,
    // is only given by the MOJ
    News
}

public class Group : BaseEntity
{
    public GroupTypes GroupType { get; set; }
    public List<GroupFlag> Flags { get; set; }
    public bool Open { get; set; }
    public List<long> Invited { get; set; }
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

    public List<GroupRole> Roles { get; set; }

    public long OwnerId { get; set; }

    public bool IsInGroup(SVUser user)
    {
        return MembersIds.Contains(user.Id);
    }

    /// <summary>
    /// Returns the item for the given id
    /// </summary>
    public static async ValueTask<Group> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<Group>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<Group>($"api/groups/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public async ValueTask<BaseEntity> GetOwnerAsync()
    {
        return await BaseEntity.FindAsync(OwnerId);
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