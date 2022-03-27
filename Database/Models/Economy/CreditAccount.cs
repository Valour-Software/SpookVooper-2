using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Users;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy;


public class CreditAccount : IHasOwner, IEntity
{
    [Key]
    public string Id { get; set;}
    public string Name { get; }
    public string Description { get; set; }

    [NotMapped]
    public string Image_Url { get; set; }
    public string OwnerId { get; set;}
    [ForeignKey("OwnerId")]
    public IEntity Owner { get; set; }

    [JsonIgnore]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
    // used for tax purposes
    public decimal CreditsYesterday { get; set;}
}