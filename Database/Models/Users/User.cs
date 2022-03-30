using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Users;

public enum Rank
{
    Spleen = 1,
    Carb = 2,
    Gaty = 3,
    Corgi = 4,
    Oof = 5,
    Unranked = 6
}

public class User : IEntity
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Xp { get; set;}
    public int Messages { get; set;}
    public DateTime LastSentMessage { get; set;}

    [JsonIgnore]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
    public decimal CreditsYesterday { get; set;}
    public Rank Rank { get; set;}
    // the datetime that this user created their account
    public DateTime Created { get; set; }
    public string Image_Url { get; set; }
    public string? DistrictId { get; set;}
    public static async Task<User?> FindAsync(string Id)
    {
        if (DBCache.Contains<User>(Id)) {
            return DBCache.Get<User>(Id);
        }
        User? user = await VooperDB.Instance.Users.FindAsync(Id);
        await DBCache.Put<User>(Id, user);
        return user;
    }
}