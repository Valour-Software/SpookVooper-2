using System.Text.Json.Serialization;

namespace Shared.Models.Districts;

public enum TerrainType
{
    Plains = 1,
    Mountain = 2,
    Hills = 3,
    Urban = 4,
    Forests = 5,
    River = 6,
    Marsh = 7
}

public class ProvinceConsumerGoodsData
{
    public string ConsumerGood { get; set; }
    public double AmountNeeded { get; set; }
    public double BuffToBirthRate { get; set; }
    public double BuffToGrowth { get; set; }
}

public class Province : Item
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public long DistrictId { get; set; }
    public District District { get; set; }
    public long? CityId { get; set; }
    public int BuildingSlots { get; set; }
    public long Population { get; set; }
    public string? Description { get; set; }
    public long? GovernorId { get; set; }
    public long? StateId { get; set; }

    /// <summary>
    /// How "developed" this province is
    /// </summary>
    public int DevelopmentValue { get; set; }
    public int BaseDevelopmentValue { get; set; }
    public int LastTickDevelopmentValue { get; set; }
    public int MigrationAttraction { get; set; }

    /// <summary>
    /// In monthly rate
    /// </summary>
    public double? BasePropertyTax { get; set; }

    /// <summary>
    /// In monthly rate
    /// </summary>
    public double? PropertyTaxPerSize { get; set; }
    public Dictionary<ProvinceModifierType, ProvinceModifier> Modifiers { get; set; }

    public List<StaticModifier> StaticModifiers { get; set; }
    public ProvinceMetadata Metadata { get; set; }
    public int MonthlyEstimatedMigrants { get; set; }
    public int RankByDevelopment { get; set; }
    public int RankByMigrationAttraction { get; set; }
    public int BuildingSlotsUsed { get; set; }

    public async ValueTask<State> GetStateAsync()
    {
        if (StateId is null)
            return null;
        return await State.FindAsync((int)StateId);
    }

    public async ValueTask<BaseEntity> GetGovernorAsync()
    {
        if (GovernorId is null)
            return null;
        return await BaseEntity.FindAsync((int)GovernorId);
    }

    public async ValueTask<long> GetLevelsOfBuildingsOfTypeAsync(string type)
    {
        return 0;
        //BuildingType buildingtype = Enum.Parse<BuildingType>(type, true);
        //return GetBuildings().Where(x => x.BuildingType == buildingtype).Sum(x => x.Size);
    }

    public static async ValueTask<Province> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<Province>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<Province>($"api/provinces/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}

public class ProvinceMetadata
{
    public long Id { get; set; }

    [JsonPropertyName("resources")]
    public Dictionary<string, long> Resources { get; set; }

    [JsonPropertyName("terrian")]
    public string TerrianType { get; set; }

    [JsonPropertyName("adjacencies")]
    public List<long> Adjacencies { get; set; }
}


public class ProvinceModifier
{
    public ProvinceModifierType ModifierType { get; set; }
    public double Amount { get; set; }
}

/// <summary>
/// Enum of all modifiers in the Province scope
/// "Factor" means a % effect, if something does not have "Factor" in its name then it's just adding to the modifier
/// </summary>
public enum ProvinceModifierType
{
    BuildingSlots,
    BuildingSlotsFactor,
    BuildingSlotsExponent,
    FertileLandFactor,
    MineQuantityCap,
    MineQuantityGrowthRateFactor,
    MineThroughputFactor,
    FarmQuantityCap,
    FarmQuantityGrowthRateFactor,
    FarmThroughputFactor,
    FactoryQuantityCap,
    FactoryQuantityGrowthRateFactor,
    FactoryThroughputFactor,
    FactoryEfficiencyFactor,
    FactoryEfficiency,
    AllProducingBuildingThroughputFactor,
    MigrationAttractionFactor,
    OverPopulationModifierExponent,
    OverPopulationModifierPopulationBase,
    MigrationAttraction,
    DevelopmentValue,
    ConsumerGoodsConsumptionFactor,
    ConsumerGoodsModifierFactor,
    InfrastructureThroughputFactor
}