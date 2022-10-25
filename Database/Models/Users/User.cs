using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;

namespace SV2.Database.Models.Users;

public enum Rank
{
    Spleen = 1,
    Crab = 2,
    Gaty = 3,
    Corgi = 4,
    Oof = 5,
    Unranked = 6
}

public class User : IEntity
{
    [Key]
    public long Id { get; set; }

    [Column(TypeName = "bigint")]

    public long ValourId { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(1024)]
    public string? Description { get; set; }
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

    [JsonIgnore]
    [VarChar(36)]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
    public decimal TaxAbleCredits { get; set; }
    public List<decimal>? CreditSnapshots { get; set;}
    public Rank Rank { get; set;}

    // the datetime that this user created their account
    public DateTime Created { get; set; }

    public DateTime LastSentMessage { get; set; }

    [VarChar(128)]
    public string? Image_Url { get; set; }

    [NotMapped]
    public float Xp
    {
        get
        {
            return MessageXp + ForumXp;
        }
    }

    public long DistrictId { get; set;}
    public EntityType entityType {
        get {
            return EntityType.User;
        }
    }

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }

    public void NewMessage(PlanetMessage msg)
    {
        if (LastSentMessage.AddSeconds(60) < DateTime.UtcNow)
        {
            double xpgain = (Math.Log10(PointsThisMinute) - 1) * 3;
            xpgain = Math.Max(0.2, xpgain);
            MessageXp += (float)xpgain;
            ActiveMinutes += 1;
            PointsThisMinute = 0;
            LastSentMessage = DateTime.UtcNow;
        }

        string Content = RemoveWhitespace(msg.Content);

        Content = Content.Replace("*", "");

        short Points = 0;

        // each char grants 1 point
        Points += (short)Content.Length;

        // if there is media then add 100 points
        if (Content.Contains("https://vmps.valour.gg"))
        {
            Points += 100;
        }

        PointsThisMinute += Points;
        TotalChars += Content.Length;
        TotalPoints += Points;

        Messages += 1;
    }

    public bool IsMinister(MinisterType ministertype)
    {
        Minister? minister = DBCache.GetAll<Minister>().FirstOrDefault(x => x.UserId == Id && x.Type == ministertype);
        if (minister is null) {
            return false;
        }
        return true;
    }

    public static User? FindByName(string name)
    {
        return DBCache.GetAll<User>().FirstOrDefault(x => x.Name == name);
    }
    
    public static User? Find(long Id)
    {
        return DBCache.Get<User>(Id);
    }

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        if (apikey == Api_Key) {
            return true;
        }
        return false;
    }

    public bool HasPermission(IEntity entity, GroupPermission permission)
    {
        if (entity.Id == Id) {
            return true;
        }
        return false;
    }

    public User(string name, long valourId)
    {
        Id = IdManagers.UserIdGenerator.Generate();
        ValourId = valourId;
        Name = name;
        ForumXp = 0;
        MessageXp = 0;
        Messages = 0;
        PostLikes = 0;
        CommentLikes = 0;
        Api_Key = Guid.NewGuid().ToString();
        Credits = 0.0m;
        CreditSnapshots = new();
        Rank = Rank.Unranked;
        Created = DateTime.UtcNow;
    }
}