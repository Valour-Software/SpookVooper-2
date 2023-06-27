using Shared.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Models.Military;

public enum DivisionModifierType
{
    Attack = 0,
    Health = 1,
    Speed = 2
}
public class DivisionTemplate
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public string Name { get; set; }
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
    public long AmmoWeaponItemDefinitionId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public Dictionary<DivisionModifierType, double> Modifiers { get; set; }

    public async ValueTask<ItemDefinition> GetWeaponItemDefinitionAsync() => await ItemDefinition.FindAsync(WeaponItemDefinitionId);

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
        if (ItemDefinitionId == 0)
            return;

        foreach (var pair in (await GetItemDefinitionAsync())!.Modifiers)
            UpdateOrAddModifier(ConvertItemModifierToDivisionModifier[pair.Key], pair.Value * Count);
    }
}