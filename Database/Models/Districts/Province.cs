using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Managers;
using SV2.NonDBO;
using SV2.Managers;
using SV2.Scripting.LuaObjects;
using SV2.Database.Models.Users;
using SV2.Scripting;

namespace SV2.Database.Models.Districts;

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

public class Province
{
    [Key]
    public long Id { get; set; }

    [VarChar(64)]
    public string? Name { get; set; }

    public long DistrictId { get; set; }

    [NotMapped]
    public District District { get; set; }

    public long? CityId { get; set; }

    public int BuildingSlots { get; set; }

    public long Population { get; set; }

    public string? Description { get; set; }

    public long? GovernorId { get; set; }

    [NotMapped]
    public BaseEntity? Governor => BaseEntity.Find(GovernorId);

    public City? City
    {
        get
        {
            if (CityId is null) return null;
            return DBCache.Get<City>(CityId);
        }
    }

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

    [NotMapped]
    public ProvinceDevelopmentStage CurrentDevelopmentStage { get; set; }

    [NotMapped]
    public Dictionary<ProvinceModifierType, ProvinceModifier> Modifiers { get; set; }

    [Column(TypeName = "jsonb")]
    public List<StaticProvinceModifier> StaticProvinceModifiers { get; set; }

    [NotMapped]
    public ProvinceMetadata Metadata => ProvinceManager.ProvincesMetadata[Id];

    [NotMapped]
    public int MonthlyEstimatedMigrants { get; set; }

    [NotMapped]
    public int RankByDevelopment { get; set; }

    public Province() { }

    public Province(Random rnd)
    {
        StaticProvinceModifiers = new();
        Modifiers = new();
        long min = (long)Defines.NProvince[NProvince.BASE_POPULATION_MIN];
        long max = (long)Defines.NProvince[NProvince.BASE_POPULATION_MAX];
        Population = rnd.NextInt64(min, max);
    }

    public string GetDevelopmentColorForMap()
    {
        DevelopmentMapColor currentmapcolor = null;
        DevelopmentMapColor nextmapcolor = null;

        int index = 0;
        while (nextmapcolor is null || nextmapcolor.MaxValue < DevelopmentValue)
        {
            currentmapcolor = nextmapcolor;
            nextmapcolor = ProvinceManager.DevelopmentMapColors[index];
            index += 1;
        }

        Color color = new(0, 0, 0);
        if (currentmapcolor is not null)
        {
            int diff = nextmapcolor.MaxValue - currentmapcolor.MaxValue;
            float progress = ((float)(DevelopmentValue - currentmapcolor.MaxValue) / (float)diff);
            color = new()
            {
                R = (int)(currentmapcolor.color.R * (1 - progress)),
                G = (int)(currentmapcolor.color.G * (1 - progress)),
                B = (int)(currentmapcolor.color.B * (1 - progress))
            };

            color.R += (int)(nextmapcolor.color.R * progress);
            color.G += (int)(nextmapcolor.color.G * progress);
            color.B += (int)(nextmapcolor.color.B * progress);
        }
        else
        {
            color = new(nextmapcolor.color.R, nextmapcolor.color.G, nextmapcolor.color.B);
        }

        if (color.R > 255) { color.R = 255; }
        if (color.G > 255) { color.G = 255; }
        if (color.B > 255) { color.B = 255; }

        return $"rgb({color.R}, {color.G}, {color.B})";
    }

    [NotMapped]
    public int BuildingSlotsUsed => GetBuildings().Where(x => x.BuildingObj.UseBuildingSlots).Count();

    public IEnumerable<BuildingBase> GetBuildings()
    {
        List<BuildingBase> buildings = new();
        buildings.AddRange(DBCache.GetAll<Factory>().Where(x => x.ProvinceId == Id));
        buildings.AddRange(DBCache.GetAll<Mine>().Where(x => x.ProvinceId == Id));
        return buildings;
    }

    public bool CanManageBuildingRequests(BaseEntity entity) {
        if (entity.Id == District.GovernorId) return true;
        if (Governor is not null) {
            if (Governor.EntityType == EntityType.User)
                return GovernorId == entity.Id;
            else {
                Group governorasgroup = (Group)Governor;
                return governorasgroup.HasPermission(entity, GroupPermissions.ManageBuildingRequests);
            }
        }
        return false;
    }

    public bool CanEdit(BaseEntity entity)
    {
        if (entity.Id == District.GovernorId) return true;
        if (Governor is not null)
        {
            if (Governor.EntityType == EntityType.User)
                return GovernorId == entity.Id;
            else
            {
                Group governorasgroup = (Group)Governor;
                return governorasgroup.HasPermission(entity, GroupPermissions.ManageProvinces);
            }
        }
        return false;
    }

    public double GetModifierValue(ProvinceModifierType modifierType) {
        if (!Modifiers.ContainsKey(modifierType))
            return 0;
        return Modifiers[modifierType].Amount;
    }

    public double GetOverpopulationModifier()
    {
        var exponent = Defines.NProvince[NProvince.OVERPOPULATION_MODIFIER_EXPONENT];
        exponent += GetModifierValue(ProvinceModifierType.OverPopulationModifierExponent);
        exponent += District.GetModifierValue(DistrictModifierType.OverPopulationModifierExponent);
        var population = Population + GetModifierValue(ProvinceModifierType.OverPopulationModifierPopulationBase);
        var rate = Math.Pow(population, exponent) / 100.0;
        rate += Defines.NProvince[NProvince.OVERPOPULATION_MODIFIER_BASE];
        if (rate > 0)
            return rate;
        return 0.00;
    }

    public double GetMonthlyPopulationGrowth()
    {
        double BirthRate = Defines.NProvince["BASE_BIRTH_RATE"];
        BirthRate += District.GetModifierValue(DistrictModifierType.MonthlyBirthRate);
        BirthRate *= District.GetModifierValue(DistrictModifierType.MonthlyBirthRateFactor) + 1;

        double DeathRate = Defines.NProvince["BASE_DEATH_RATE"];
        DeathRate += District.GetModifierValue(DistrictModifierType.MonthlyDeathRate);
        DeathRate *= District.GetModifierValue(DistrictModifierType.MonthlyDeathRateFactor) + 1;

        var rate = GetOverpopulationModifier();
        if (rate > 0)
            DeathRate += rate;

        double PopulationGrowth = BirthRate * Population;
        PopulationGrowth -= DeathRate * Population;
        return PopulationGrowth;
    }

    public int GetMigrationAttraction()
    {
        double attraction = Defines.NProvince[NProvince.BASE_MIGRATION_ATTRACTION];
        attraction += Math.Max(Math.Pow(DevelopmentValue, Defines.NProvince[NProvince.MIGRATION_DEVELOPMENT_EXPONENT]) / Defines.NProvince[NProvince.MIGRATION_DEVELOPMENT_DIVISOR] + Defines.NProvince[NProvince.MIGRATION_DEVELOPMENT_BASE], 0);
        attraction += Math.Max(Math.Pow(BuildingSlots, Defines.NProvince[NProvince.MIGRATION_BUILDINGSLOTS_EXPONENT]) / Defines.NProvince[NProvince.MIGRATION_BUILDINGSLOTS_DIVISOR] + Defines.NProvince[NProvince.MIGRATION_BUILDINGSLOTS_BASE], 0);

        // apply bonuses based on ranking by dev value
        if (District.ProvincesByDevelopmnet[14].DevelopmentValue <= DevelopmentValue)
        {
            int rank = District.ProvincesByDevelopmnet.IndexOf(this);
            attraction += 3;
            attraction *= 1.15;
        }

        attraction *= GetModifierValue(ProvinceModifierType.MigrationAttractionFactor) + 1;

        if (GetOverpopulationModifier() > 0.25)
        {
            var muit = (GetOverpopulationModifier() - 0.25) * 3;
            muit = Math.Min(muit, 0.6);
            attraction *= muit;
        }

        return (int)attraction;
    }

    public async ValueTask HourlyTick()
    {
        if (Population < 2500) Population = 2500;
        // update modifiers now
        UpdateModifiers();

        DevelopmentValue = (int)(Math.Floor(Math.Pow(Population, Defines.NProvince[NProvince.DEVELOPMENT_POPULATION_EXPONENT])) * Defines.NProvince[NProvince.DEVELOPMENT_POPULATION_FACTOR]);
        
        BaseDevelopmentValue = DevelopmentValue;
        bool hasdonecoastalbonus = false;

        foreach (var id in Metadata.Adjacencies)
        {
            var _metadata = ProvinceManager.ProvincesMetadata[id];
            if (_metadata.TerrianType == "ocean" && hasdonecoastalbonus) continue;

            if (_metadata.TerrianType == "ocean" && !hasdonecoastalbonus)
            {
                DevelopmentValue += (int)Defines.NProvince[NProvince.DEVELOPMENT_COASTAL_BONUS];
                DevelopmentValue += (int)(Defines.NProvince[NProvince.DEVELOPMENT_COASTAL_FACTOR] * BaseDevelopmentValue);
                hasdonecoastalbonus = true;
            }
            var adj_province = DBCache.Get<Province>(id);
            if (adj_province is null || BaseDevelopmentValue > adj_province.BaseDevelopmentValue) continue;

            DevelopmentValue += (int)(adj_province.LastTickDevelopmentValue * 0.1);
        }

        LastTickDevelopmentValue = DevelopmentValue;

        RankByDevelopment = District.ProvincesByDevelopmnet.IndexOf(this);

        int currenthighestvalue = 0;
        int index = 0;
        var stages = GameDataManager.ProvinceDevelopmentStages.Values.ToList();
        ProvinceDevelopmentStage higheststage = stages[0];
        while (currenthighestvalue < DevelopmentValue)
        {
            if (index > stages.Count - 1) break;
            var stage = stages[index];
            if (DevelopmentValue < stage.DevelopmentLevelNeeded || index > stages.Count - 1)
                break;
            higheststage = stage;
            currenthighestvalue = stage.DevelopmentLevelNeeded;
            index++;
        }
        if (CurrentDevelopmentStage is null || CurrentDevelopmentStage.Name != higheststage.Name)
        {
            CurrentDevelopmentStage = higheststage;
            UpdateModifiers();
        }

        CurrentDevelopmentStage = higheststage;

        foreach (var building in DBCache.ProvincesBuildings[Id]) {
            await building.Tick();
            await building.TickRecipe();
        }
        
        UpdateModifiersAfterBuildingTick();

        // get hourly rate
        var PopulationGrowth = GetMonthlyPopulationGrowth() / 30 / 24;
        Population += (long)Math.Ceiling(PopulationGrowth);

        // update building slot count

        double buildingslots_exponent = Defines.NProvince["BUILDING_SLOTS_POPULATION_EXPONENT"];
        buildingslots_exponent += GetModifierValue(ProvinceModifierType.BuildingSlotsExponent);
        buildingslots_exponent += District.GetModifierValue(DistrictModifierType.BuildingSlotsExponent);

        BuildingSlots = (int)(Defines.NProvince["BASE_BUILDING_SLOTS"] + Math.Ceiling((Math.Pow(Population, buildingslots_exponent) * Defines.NProvince["BUILDING_SLOTS_FACTOR"])));
        
        // province level
        BuildingSlots += (int)GetModifierValue(ProvinceModifierType.BuildingSlots);
        var buildingSlots_factor = 1 + GetModifierValue(ProvinceModifierType.BuildingSlotsFactor);

        // district level
        buildingSlots_factor += District.GetModifierValue(DistrictModifierType.BuildingSlotsFactor);

        BuildingSlots = (int)(BuildingSlots * buildingSlots_factor);

        MigrationAttraction = GetMigrationAttraction();
    }

    public void UpdateOrAddModifier(ProvinceModifierType type, double value)
    {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = new() { Amount = value, ModifierType = type};
        else
            Modifiers[type].Amount += value;
    }

    public void UpdateModifiers()
    {
        Modifiers = new();
        var value_executionstate = new ExecutionState(District, this);
        var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var staticmodifier in StaticProvinceModifiers)
        {
            foreach (var modifiernode in staticmodifier.luaStaticModifierObject.ModifierNodes)
            {
                var value = (double)modifiernode.GetValue(value_executionstate, staticmodifier.ScaleByNode.GetValue(scaleby_executionstate));
                UpdateOrAddModifier((ProvinceModifierType)modifiernode.ProvinceModifierType!, value);
            }
        }

        if (CurrentDevelopmentStage is not null)
        {
            value_executionstate = new ExecutionState(District, this);
            foreach (var modifiernode in CurrentDevelopmentStage.ModifierNodes)
            {
                var value = (double)modifiernode.GetValue(value_executionstate, 1);
                UpdateOrAddModifier((ProvinceModifierType)modifiernode.ProvinceModifierType!, value);
            }
        }
    }

    public void UpdateModifiersAfterBuildingTick() {
        var buildingtick_executionstate = new ExecutionState(District, this);
        foreach (var building in DBCache.ProvincesBuildings[Id]) {
            if (!building.SuccessfullyTicked) continue;
            if (building.Recipe.ModifierNodes is null) continue;
            foreach (var modifiernode in building.Recipe.ModifierNodes) {
                var value = (double)modifiernode.GetValue(buildingtick_executionstate, 1);
                value *= building.GetRateForProduction();
                UpdateOrAddModifier((ProvinceModifierType)modifiernode.ProvinceModifierType!, value);
            }
        }
    }
}

public class ProvinceModifier
{
    public ProvinceModifierType ModifierType { get; set; }
    public double Amount { get; set; }
}

public class StaticProvinceModifier
{
    public string Id { get; set; }
    public long? Duration { get; set; }
    public bool Decay { get; set; }
    public ExpressionNode? ScaleByNode { get; set; }
    public DateTime TimeStarted { get; set; }
    public string StaticModifierId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public LuaProvinceStaticModifier luaStaticModifierObject => GameDataManager.BaseProvinceStaticModifers[StaticModifierId];
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
    OverPopulationModifierPopulationBase
}