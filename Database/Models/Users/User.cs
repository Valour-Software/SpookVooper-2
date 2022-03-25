using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Users;


public class User : Entity
{
    public int Xp { get; set;}
    public int Messages { get; set;}
    public DateTime LastSentMessage { get; set;}
    [JsonIgnore]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
    // used for tax purposes
    public decimal CreditsYesterday { get; set;}
}