using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using SV2.Web;
using System.Text.Json.Serialization;

namespace SV2.Database.Models.Items;

public class SVItemOwnership : IHasOwner
{
    [Key]
    [Column("id")]
    public long Id {get; set; }

    [Column("ownerid")]
    public long OwnerId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;
    
    [Column("definitionid")]
    public long DefinitionId { get; set; }
    
    [NotMapped]
    [JsonIgnore]
    public ItemDefinition Definition => DBCache.Get<ItemDefinition>(DefinitionId)!;

    [Column("amount", TypeName = "numeric(16, 2)")]
    public double Amount { get; set;}
}