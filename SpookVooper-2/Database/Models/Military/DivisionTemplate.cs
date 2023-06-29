using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SV2.Database.Models.Military;

public class DivisionTemplate
{
    [Key]
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public string Name { get; set; }

    [Column(TypeName = "jsonb[]")]
    public List<RegimentTemplate> RegimentsTemplates { get; set; }

    [NotMapped]
    public Dictionary<DivisionModifierType, double> Modifiers { get; set; }

    public void UpdateOrAddModifier(DivisionModifierType type, double value)
    {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = value;
        else
            Modifiers[type] += value;
    }

    public async ValueTask UpdateModifiers()
    {
        Modifiers = new();

        foreach (var regiment in RegimentsTemplates)
        {
            foreach (var pair in regiment.Modifiers)
                UpdateOrAddModifier(pair.Key, pair.Value);
        }
    }
}

public class RegimentTemplate
{
    public long Id { get; set; }
    public RegimentType Type { get; set; }

    // number of things in this regiment
    // for example in an Infantry Regiment, Count will be the number of soldiers
    // only allowed values are in 1k increments for infantry and 1 increments for everything else
    public int Count { get; set; }
    public long WeaponItemDefinitionId { get; set; }
    public long AmmoItemDefinitionId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public ItemDefinition WeaponItemDefinition => DBCache.Get<ItemDefinition>(WeaponItemDefinitionId)!;

    [NotMapped]
    [JsonIgnore]
    public ItemDefinition AmmoItemDefinition => DBCache.Get<ItemDefinition>(AmmoItemDefinitionId)!;

    [NotMapped]
    [JsonIgnore]
    public Dictionary<DivisionModifierType, double> Modifiers { get; set; }

    public void UpdateOrAddModifier(DivisionModifierType type, double value)
    {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = value;
        else
            Modifiers[type] += value;
    }

    public static Dictionary<ItemModifierType, DivisionModifierType> ConvertItemModifierToDivisionModifier = new()
    {
        { ItemModifierType.Attack, DivisionModifierType.Attack }
    };

    public async ValueTask UpdateModifiers()
    {
        Modifiers = new();

        foreach (var pair in WeaponItemDefinition.Modifiers)
            UpdateOrAddModifier(ConvertItemModifierToDivisionModifier[pair.Key], pair.Value*Count);

        foreach (var pair in AmmoItemDefinition.Modifiers)
            UpdateOrAddModifier(ConvertItemModifierToDivisionModifier[pair.Key], pair.Value * Count);
    }
}