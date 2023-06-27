using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SV2.Database.Models.Items;

public class ItemDefinition : IHasOwner 
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ownerid")]
    public long OwnerId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created")]
    public DateTime Created { get; set; }

    [NotMapped]
    public Dictionary<ItemModifierType, double>? Modifiers { get; set; }

    /// <summary>
    /// For example, if this was a NVTech Tank, the base item would be the SV Tank item definition
    /// </summary>
    [Column("baseitemdefinitionid")]
    public long? BaseItemDefinitionId { get; set; }
    public bool Transferable { get; set; }

    [NotMapped]
    public string? BaseItemName => BaseItemDefinitionId is null ? null : DBCache.Get<ItemDefinition>(BaseItemDefinitionId).Name;

    [NotMapped]
    [JsonIgnore]
    public bool IsSVItem => OwnerId == 100 || BaseItemDefinitionId is not null;

    public void UpdateOrAddModifier(ItemModifierType type, double value)
    {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = value;
        else
            Modifiers[type] += value;
    }

    public async ValueTask UpdateModifiers()
    {
        Modifiers = new();
        if (BaseItemDefinitionId is not null)
        {
            var recipe = DBCache.GetAll<Recipe>().FirstOrDefault(x => x.CustomOutputItemDefinitionId == Id);
            Modifiers = recipe.Modifiers;
        }
        else
        {
            Modifiers = Name switch
            {
                "Normal Ammo" => new()
                {
                    { ItemModifierType.Attack, 0.75}
                },
                "Crystallite Infused Ammo" => new()
                {
                    { ItemModifierType.Attack, 1.5 },
                    { ItemModifierType.AttackFactor, 0.25 }
                },
                _ => new()
            };
        }
    }

    public ItemDefinition() {

    }

    public ItemDefinition(long ownerid, string name) {
        Id = IdManagers.GeneralIdGenerator.Generate();
        OwnerId = ownerid;
        Name = name;
        Created = DateTime.UtcNow;
    }


}