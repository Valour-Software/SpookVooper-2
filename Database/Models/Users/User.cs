using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Users;

public enum Rank
{
    Spleen,
    Carb,
    Gaty,
    Corgi,
    Oof,
    Unranked
}

public class User : IEntity
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Image_Url { get; set; }
    public decimal Credits { get; set;}
    public decimal CreditsYesterday { get; set;}
    public int Xp { get; set;}
    public int Messages { get; set;}
    public DateTime LastSentMessage { get; set;}

    [JsonIgnore]
    public string Api_Key { get; set; }
    public Rank Rank { get; set;}
    // the datetime that this user created their account
    public DateTime Created { get; set; }
    public string PfpUrl { get; set; }
}