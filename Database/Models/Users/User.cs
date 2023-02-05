using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;
using Valour.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

[Table("users")]
public class SVUser : BaseEntity
{
    [BigInt]
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
    // the datetime that this user created their account
    public DateTime Created { get; set; }

    public DateTime LastSentMessage { get; set; }

    [NotMapped]
    public float Xp => MessageXp + ForumXp;

    public override EntityType EntityType => EntityType.User;

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }

    public void NewMessage(PlanetMessage msg)
    {
        if (LastActiveMinute.AddSeconds(60) < DateTime.UtcNow)
        {
            if (PointsThisMinute <= 3)
                PointsThisMinute += 3;
            double xpgain = (Math.Log10(PointsThisMinute) - 1) * 3;
            xpgain = Math.Max(0.2, xpgain);
            MessageXp += (float)xpgain;
            ActiveMinutes += 1;
            PointsThisMinute = 0;
            LastSentMessage = DateTime.UtcNow;
            LastActiveMinute = DateTime.UtcNow;
        }

        string Content = RemoveWhitespace(msg.Content);

        Content = Content.Replace("*", "");

        short Points = 0;

        // each char grants 1 point
        Points += (short)Content.Length;

        // if there is media then add 150 points
        if (msg.AttachmentsData.Contains("https://cdn.valour.gg/content/"))
        {
            Points += 150;
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

    public static SVUser? FindByName(string name)
    {
        return DBCache.GetAll<SVUser>().FirstOrDefault(x => x.Name == name);
    }
    
    public static SVUser? Find(long Id)
    {
        return DBCache.Get<SVUser>(Id);
    }

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        if (apikey == ApiKey) {
            return true;
        }
        return false;
    }

    public bool HasPermission(BaseEntity entity, GroupPermission permission)
    {
        if (entity.Id == Id) {
            return true;
        }
        return false;
    }

    public SVUser(string name, long valourId)
    {
        Id = IdManagers.UserIdGenerator.Generate();
        ValourId = valourId;
        Name = name;
        ForumXp = 0;
        MessageXp = 0;
        Messages = 0;
        PostLikes = 0;
        CommentLikes = 0;
        ApiKey = Guid.NewGuid().ToString();
        Credits = 0.0m;
        CreditSnapshots = new();
        Rank = Rank.Unranked;
        Created = DateTime.UtcNow;
        Joined = DateTime.UtcNow;
    }

    public async Task<IEnumerable<Group>> GetJoinedGroupsAsync()
    {
        using var dbctx = VooperDB.DbFactory.CreateDbContext();
        var groups = await dbctx.Groups.Where(x => x.MembersIds.Contains(Id)).ToListAsync();

        return groups;
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

    public async Task CheckRoles(PlanetMember member)
    {
        var roles = await member.GetRolesAsync();
        // check rank role
        var rankname = Rank.ToString();
        if (!roles.Any(x => x.Name == rankname))
            await member.Node.PostAsync($"api/members/{member.Id}/roles/{VoopAI.VoopAI.RankRoleIds[rankname]}", null);
        if (roles.Any(x => VoopAI.VoopAI.RankNames.Contains(x.Name) && x.Name != rankname))
            await member.Node.DeleteAsync($"api/members/{member.Id}/roles/{VoopAI.VoopAI.RankRoleIds[rankname]}");

        var districtrole = VoopAI.VoopAI.DistrictRoles[District.Name + " District"];
        if (!roles.Any(x => x.Id == districtrole.Id))
            await member.Node.PostAsync($"api/members/{member.Id}/roles/{districtrole.Id}", null);
        if (roles.Any(x => VoopAI.VoopAI.DistrictRoles.ContainsKey(x.Name) && x.Id != districtrole.Id))
            await member.Node.DeleteAsync($"api/members/{member.Id}/roles/{districtrole.Id}");
    }
}