using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using SV2.Web;

namespace SV2.Database.Models.Items;

public class SVItemOwnership : IHasOwner
{
    [Key]
    [Column("id")]
    public long Id {get; set; }

    [Column("ownerid")]
    public long OwnerId { get; set; }

    [NotMapped]
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;
    
    [Column("definitionid")]
    public long DefinitionId { get; set; }
    
    [NotMapped]
    public ItemDefinition Definition => DBCache.Get<ItemDefinition>(DefinitionId)!;

    [Column("amount", TypeName = "numeric(16, 2)")]
    public double Amount { get; set;}
}