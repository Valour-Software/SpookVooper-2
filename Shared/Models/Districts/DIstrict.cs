using Shared.Models.Districts.Modifiers;

namespace Shared.Models.Districts;

public class DistrictModifier
{
    public DistrictModifierType ModifierType { get; set; }
    public double Amount { get; set; }  
}

public class District : Item
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string ScriptName => Name.ToLower().Replace(" ", "_");
    public string? Description { get; set; }
    public List<Province> Provinces { get; set; }
    public List<State> States { get; set; }
    public long TotalPopulation
    {
        get
        {
            if (Provinces is not null)
                return Provinces.Sum(x => x.Population);
            return 0;
        }
    }

    public Group Group { get; set; }
    public long GroupId { get; set; }
    public string? FlagUrl { get; set; }
    public List<StaticModifier> StaticModifiers { get; set; }
    public string? TitleForProvince { get; set; }
    public string? TitleForState { get; set; }
    public string? TitleForGovernorOfProvince { get; set; }
    public string? TitleForGovernorOfState { get; set; }
    public string NameForState => TitleForState is null ? "State" : TitleForState;
    public string NameForProvince => TitleForProvince is null ? "Province" : TitleForProvince;
    public string NameForGovernorOfAProvince => TitleForGovernorOfProvince is null ? "Governor" : TitleForGovernorOfProvince;
    public string NameForGovernorOfAState => TitleForGovernorOfState is null ? "Governor" : TitleForGovernorOfState;
    public Dictionary<DistrictModifierType, DistrictModifier> Modifiers { get; set; }
    public double EconomicScore { get; set; }
    public string Color => Name switch
    {
        "Lanatia" => "F4B7FD",
        "New Vooperis" => "FEEAB7",
        "Elysian Katonia" => "B8B7FD",
        "Ardenti Terra" => "B7BCFC",
        "Landing Cove" => "FDB7B7",
        "New Avalon" => "D3FCB6",
        "Novastella" => "B7FDE5",
        "Old King" => "C0FDB7",
        "San Vooperisco" => "FAFDB8",
        "Thesonica" => "FDD9B7",
        "Voopmont" => "FFFFFF",
    };

    public double GetModifierValue(DistrictModifierType modifierType)
    {
        if (!Modifiers.ContainsKey(modifierType))
            return 0;
        return Modifiers[modifierType].Amount;
    }

    /// <summary>
    /// Returns the item for the given id
    /// </summary>
    public static async ValueTask<District> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<District>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<District>($"api/districts/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        // put provinces and states into cache
        foreach (var province in item.Provinces)
            await SVCache.Put(province.Id, province, true);
        foreach (var state in item.States)
            await SVCache.Put(state.Id, state, true);

        return item;
    }

    public void UpdateOrAddModifier(DistrictModifierType type, double value) {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = new() { Amount = value, ModifierType = type };
        else
            Modifiers[type].Amount += value;
    }

    public void UpdateModifiers() {
        Modifiers = new();
        var value_executionstate = new ExecutionState(this, null, parentscopetype:ScriptScopeType.District);
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var staticmodifier in StaticModifiers) {
            foreach (var modifiernode in staticmodifier.BaseStaticModifiersObj.ModifierNodes) {
                var value = (double)modifiernode.GetValue(value_executionstate, staticmodifier.ScaleBy);
                UpdateOrAddModifier((DistrictModifierType)modifiernode.districtModifierType!, value);
            }
        }
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}