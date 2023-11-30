namespace Shared.Models.Users;

public enum Rank
{
    Spleen = 1,
    Crab = 2,
    Gaty = 3,
    Corgi = 4,
    Oof = 5,
    Unranked = 6
}

public class SVUser : BaseEntity
{
    public long ValourId { get; set; }
    public int ForumXp { get; set;}
    public float MessageXp { get; set;}
    public int CommentLikes { get; set;}
    public int PostLikes { get; set;}
    public int Messages { get; set;}

    // xp calc stuff

    public float PointsTotal { get; set; }
    public int ActiveMinutes { get; set; }
    public short PointsThisMinute { get; set; }
    public int TotalPoints { get; set; }
    public int TotalChars { get; set; }
    public DateTime LastActiveMinute { get; set; }
    public DateTime Joined { get; set;}
    public Rank Rank { get; set;}
    public DateTime Created { get; set; }
    public DateTime LastSentMessage { get; set; }
    public DateTime LastMoved { get; set; }
    public float Xp => MessageXp + ForumXp;
    public override EntityType EntityType => EntityType.User;

    /// <summary>
    /// Returns the item for the given id
    /// </summary>
    public static async ValueTask<SVUser> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<SVUser>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<SVUser>($"api/users/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }
}