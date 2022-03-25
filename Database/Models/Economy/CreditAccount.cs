using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Users;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy;


public class CreditAccount : Entity, IEntity
{
    public string Owner_Id { get; set;}
    [NotMapped]
    public Entity Owner {get; set;}
    [JsonIgnore]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
}