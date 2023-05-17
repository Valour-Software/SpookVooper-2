using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SV2.Database.Models.Items;

public enum ItemModifierTypes {
    Attack = 0
}

public class ItemModifier 
{
    public ItemModifierTypes Type { get; set; }
    public double Amount { get; set; }
}

public class ItemDefinition : IHasOwner 
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ownerid")]
    public long OwnerId { get; set; }

    [NotMapped]
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created")]
    public DateTime Created { get; set; }

    [Column("modifiers", TypeName = "jsonb")]
    public List<ItemModifier>? Modifiers { get; set; }

    /// <summary>
    /// For example, if this was a NVTech Tank, the base item would be the SV Tank item definition
    /// </summary>
    [Column("baseitemdefinitionid")]
    public long? BaseItemDefinitionId { get; set; }

    public bool Transferable { get; set; }

    [NotMapped]
    public bool IsSVItem => OwnerId == 100 || BaseItemDefinitionId is not null;

    public ItemDefinition() {

    }

    public ItemDefinition(long ownerid, string name) {
        Id = IdManagers.GeneralIdGenerator.Generate();
        OwnerId = ownerid;
        Name = name;
        Created = DateTime.UtcNow;
    }
}