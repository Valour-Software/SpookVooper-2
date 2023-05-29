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

public class ProvinceConsumerGoodsData
{
    public string ConsumerGood { get; set; }
    public double AmountNeeded { get; set; }
    public double BuffToBirthRate { get; set; }
    public double BuffToGrowth { get; set; }
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

    public long? StateId { get; set; }

    [NotMapped]
    public State? State => DBCache.Get<State>(StateId);

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
    public Dictionary<ProvinceModifierType, ProvinceModifier> Modifiers { get; set; } = new();

    [Column("staticmodifiers", TypeName = "jsonb[]")]
    public List<StaticModifier> StaticModifiers { get; set; } = new();

    [NotMapped]
    public ProvinceMetadata Metadata => ProvinceManager.ProvincesMetadata[Id];

    [NotMapped]
    public int MonthlyEstimatedMigrants { get; set; }

    [NotMapped]
    public int RankByDevelopment { get; set; }

    [NotMapped]
    public int RankByMigrationAttraction { get; set; }

    [NotMapped]
    public int BuildingSlotsUsed => GetBuildings().Where(x => x.BuildingObj.UseBuildingSlots).Sum(x => x.Size);

    public Province() { }

    public Province(Random rnd)
    {
        StaticModifiers = new();
        Modifiers = new();
        long min = (long)Defines.NProvince[NProvince.BASE_POPULATION_MIN];
        long max = (long)Defines.NProvince[NProvince.BASE_POPULATION_MAX];
        Population = rnd.NextInt64(min, max);
    }

    public List<(string modifiername, double value)> GetStaticModifiersOfTypes(List<ProvinceModifierType?>? provincetypes, List<DistrictModifierType?> districttypes, bool AlsoUseDistrictModifiers, bool UseProvinceModifiers = true, bool IncludeDevStage = false)
    {
        if (provincetypes is null)
            provincetypes = new();
        if (districttypes is null)
            districttypes = new();
        var result = new List<(string modifiername, double value)>();
        var modifiers = new List<StaticModifier>();
        if (UseProvinceModifiers)
            modifiers.AddRange(StaticModifiers);
        if (AlsoUseDistrictModifiers)
            modifiers.AddRange(District.StaticModifiers);
        foreach (var modifier in modifiers)
        {
            foreach (var node in modifier.BaseStaticModifiersObj.ModifierNodes)
            {
                if ((provincetypes.Contains(node.provinceModifierType) && node.provinceModifierType is not null) || (districttypes.Contains(node.districtModifierType) && node.districtModifierType is not null))
                {
                    (string modifiername, double value) item = new()
                    {
                        modifiername = modifier.BaseStaticModifiersObj.Name,
                        value = (double)node.GetValue(new(District, this, null, (node.provinceModifierType is not null ? ScriptScopeType.Province : ScriptScopeType.District)))
                    };
                    if ((provincetypes.Contains(node.provinceModifierType) && node.provinceModifierType is not null && node.provinceModifierType.ToString().Contains("Factor"))
                        || (districttypes.Contains(node.districtModifierType) && node.districtModifierType is not null && node.districtModifierType.ToString().Contains("Factor")))
                    {
                        item.value += 1;
                    }
                    result.Add(item);
                }
            }
        }
        if (IncludeDevStage)
        {
            foreach (var node in CurrentDevelopmentStage.ModifierNodes)
            {
                if ((provincetypes.Contains(node.provinceModifierType) && node.provinceModifierType is not null) || (districttypes.Contains(node.districtModifierType) && node.districtModifierType is not null))
                {
                    (string modifiername, double value) item = new()
                    {
                        modifiername = CurrentDevelopmentStage.PrintableName,
                        value = (double)node.GetValue(new(District, this, null, (node.provinceModifierType is not null ? ScriptScopeType.Province : ScriptScopeType.District)))
                    };
                    if ((provincetypes.Contains(node.provinceModifierType) && node.provinceModifierType is not null && node.provinceModifierType.ToString().Contains("Factor"))
                        || (districttypes.Contains(node.districtModifierType) && node.districtModifierType is not null && node.districtModifierType.ToString().Contains("Factor")))
                    {
                        item.value += 1;
                    }
                    result.Add(item);
                }
            }
        }
        return result;
    }

    public List<(string modifiername, double value)> GetStaticModifiersOfType(ProvinceModifierType? provincetype, DistrictModifierType? districttype, bool AlsoUseDistrictModifiers, bool UseProvinceModifiers = true, bool IncludeDevStage = false) {
        var result = new List<(string modifiername, double value)>();
        var modifiers = new List<StaticModifier>();
        if (UseProvinceModifiers)
            modifiers.AddRange(StaticModifiers);
        if (AlsoUseDistrictModifiers)
            modifiers.AddRange(District.StaticModifiers);
        foreach (var modifier in modifiers) {
            foreach (var node in modifier.BaseStaticModifiersObj.ModifierNodes) {
                if ((node.provinceModifierType == provincetype && node.provinceModifierType is not null) || (node.districtModifierType == districttype && node.districtModifierType is not null)) {
                    (string modifiername, double value) item = new() {
                        modifiername = modifier.BaseStaticModifiersObj.Name,
                        value = (double)node.GetValue(new(District, this, null, (node.provinceModifierType is not null ? ScriptScopeType.Province : ScriptScopeType.District)))
                    }; 
                    if ((node.provinceModifierType == provincetype && node.provinceModifierType is not null && node.provinceModifierType.ToString().Contains("Factor") )
                        || (node.districtModifierType == districttype && node.districtModifierType is not null && node.districtModifierType.ToString().Contains("Factor"))) {
                        item.value += 1;
                    }
                    result.Add(item);
                }
            }
        }
        if (IncludeDevStage && CurrentDevelopmentStage is not null) {
            foreach (var node in CurrentDevelopmentStage.ModifierNodes) {
                if ((node.provinceModifierType == provincetype && node.provinceModifierType is not null) || (node.districtModifierType == districttype && node.districtModifierType is not null)) {
                    (string modifiername, double value) item = new() {
                        modifiername = CurrentDevelopmentStage.PrintableName,
                        value = (double)node.GetValue(new(District, this, null, (node.provinceModifierType is not null ? ScriptScopeType.Province : ScriptScopeType.District)))
                    };
                    if ((node.provinceModifierType == provincetype && node.provinceModifierType is not null && node.provinceModifierType.ToString().Contains("Factor"))
                        || (node.districtModifierType == districttype && node.districtModifierType is not null && node.districtModifierType.ToString().Contains("Factor"))) {
                        item.value += 1;
                    }
                    result.Add(item);
                }
            }
        }
        return result;
    }

    public double GetMiningResourceProduction(string resource)
    {
        if (!Metadata.Resources.ContainsKey(resource))
            return 0;
        var modifiervalue = GetModifierValue(ProvinceModifierType.MineThroughputFactor) + GetModifierValue(ProvinceModifierType.AllProducingBuildingThroughputFactor);
        modifiervalue += 1;
        return Metadata.Resources[resource] * modifiervalue;
    }

    public string GetMapColorForResourceDensity(double max, string resource, bool returnraw = false)
    {
        Color color = new(0, 0, 0);
        if (Metadata.Resources.ContainsKey(resource) && max > 0.01)
        {
            var amount = GetMiningResourceProduction(resource);
            var scale = amount / max;
            color.R = (int)(255 * scale);
            color.G = (int)(255 * scale);
            color.B = (int)(255 * scale);
        }
        if (!returnraw)
        {
            return $"rgb({color.R}, {color.G}, {color.B})";
        }
        else
        {
            return $"{color.R}, {color.G}, {color.B}";
        }
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

    public long GetLevelsOfBuildingsOfType(string type) {
        BuildingType buildingtype = Enum.Parse<BuildingType>(type, true);
        return GetBuildings().Where(x => x.BuildingType == buildingtype).Sum(x => x.Size);
    }

    public IEnumerable<BuildingBase> GetBuildings()
    {
        return DBCache.ProvincesBuildings[Id];
    }

    /// <summary>
    /// Returns the governor of this province, or if governor is null, then the state
    /// If the state's is null, then the district
    /// </summary>
    /// <returns></returns>
    public BaseEntity GetGovernor()
    {
        if (Governor is not null)
            return Governor;
        else if (State is not null)
            return State.Group;
        else
            return District.Group;
    }

    public bool CanManageBuildingRequests(BaseEntity entity) {
        if (entity.Id == District.GovernorId) return true;
        if (State is not null && State.Governor is not null) {
            if (State.Governor.EntityType == EntityType.User) {
                if (State.GovernorId == entity.Id) {
                    return true;
                }
            }
            else {
                Group governorasgroup = (Group)State.Governor;
                if (governorasgroup.HasPermission(entity, GroupPermissions.ManageBuildingRequests))
                    return true;
            }
        }
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
        if (State is not null && State.Governor is not null) {
            if (State.Governor.EntityType == EntityType.User) {
                if (State.GovernorId == entity.Id) {
                    return true;
                }
            }
            else {
                Group governorasgroup = (Group)State.Governor;
                if (governorasgroup.HasPermission(entity, GroupPermissions.ManageBuildingRequests))
                    return true;
            }
        }
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
        if (population < 2500) population = 2500;
        var rate = Math.Pow(population, exponent) / 100.0;
        rate += Defines.NProvince[NProvince.OVERPOPULATION_MODIFIER_BASE];
        if (rate > 0)
            return rate;
        return 0.00;
    }

    public async ValueTask<(double growthrate, List<ProvinceConsumerGoodsData> ConsumerGoodsData)> GetMonthlyPopulationGrowth(bool UseResources = false)
    {
        double BirthRate = Defines.NProvince["BASE_BIRTH_RATE"];
        BirthRate += District.GetModifierValue(DistrictModifierType.MonthlyBirthRate);
        BirthRate *= District.GetModifierValue(DistrictModifierType.MonthlyBirthRateFactor) + 1;

        var governor = GetGovernor();
        List<ProvinceConsumerGoodsData> consumerGoodsData = new();
        var rate_for_consumergood = (double)Population / 10_000 * (1 + GetModifierValue(ProvinceModifierType.ConsumerGoodsConsumptionFactor));
        double totalgrowthbuff = 0;
        foreach (var consumergood in GameDataManager.ConsumerGoods)
        {
            var toconsume = rate_for_consumergood * consumergood.consumerGood.PopConsumptionRate;
            var data = new ProvinceConsumerGoodsData()
            {
                ConsumerGood = consumergood.Name,
                AmountNeeded = toconsume,
                BuffToGrowth = 0,
                BuffToBirthRate = 0
            };
            if (await governor.HasEnoughResource(consumergood.LowerCaseName, toconsume))
            {
                var buff = consumergood.consumerGood.PopGrowthRateModifier * (1 + GetModifierValue(ProvinceModifierType.ConsumerGoodsModifierFactor));
                totalgrowthbuff += buff;
                BirthRate += Math.Sqrt(buff*100)/150;
                data.BuffToBirthRate = Math.Sqrt(buff * 100) / 150;
                data.BuffToGrowth = buff;
                if (UseResources)
                {
                    await governor.ChangeResourceAmount(consumergood.LowerCaseName, -toconsume, $"Consumer Good Usage for Province with name: {Name}");
                }
            }
            consumerGoodsData.Add(data);
        }

        double DeathRate = Defines.NProvince["BASE_DEATH_RATE"];
        DeathRate += District.GetModifierValue(DistrictModifierType.MonthlyDeathRate);
        DeathRate *= District.GetModifierValue(DistrictModifierType.MonthlyDeathRateFactor) + 1;

        var rate = GetOverpopulationModifier();
        if (rate > 0)
            DeathRate += rate;

        double PopulationGrowth = BirthRate * Population;
        PopulationGrowth -= DeathRate * Population;
        PopulationGrowth *= totalgrowthbuff;

        return new(PopulationGrowth, consumerGoodsData);
    }

    public int GetMigrationAttraction()
    {
        double attraction = Defines.NProvince[NProvince.BASE_MIGRATION_ATTRACTION];
        attraction += Math.Max(Math.Pow(DevelopmentValue, Defines.NProvince[NProvince.MIGRATION_DEVELOPMENT_EXPONENT]) / Defines.NProvince[NProvince.MIGRATION_DEVELOPMENT_DIVISOR] + Defines.NProvince[NProvince.MIGRATION_DEVELOPMENT_BASE], 0);
        attraction += Math.Max(Math.Pow(BuildingSlots, Defines.NProvince[NProvince.MIGRATION_BUILDINGSLOTS_EXPONENT]) / Defines.NProvince[NProvince.MIGRATION_BUILDINGSLOTS_DIVISOR] + Defines.NProvince[NProvince.MIGRATION_BUILDINGSLOTS_BASE], 0);
        attraction += GetModifierValue(ProvinceModifierType.MigrationAttraction);

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
            var muit = 1 - ((GetOverpopulationModifier() - 0.25) * 3);
            muit = Math.Max(muit, 0.6);
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
        RankByMigrationAttraction = District.ProvincesByMigrationAttraction.IndexOf(this)+1;

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

        DevelopmentValue += (int)Math.Floor(GetModifierValue(ProvinceModifierType.DevelopmentValue));

        // get hourly rate
        var PopulationGrowth = (await GetMonthlyPopulationGrowth(true)).growthrate / 30 / 24;
        Population += (long)Math.Ceiling(PopulationGrowth);

        // update building slot count

        double buildingslots_exponent = Defines.NProvince["BUILDING_SLOTS_POPULATION_EXPONENT"];
        buildingslots_exponent += GetModifierValue(ProvinceModifierType.BuildingSlotsExponent);
        buildingslots_exponent += District.GetModifierValue(DistrictModifierType.BuildingSlotsExponent);

        var slots = (Defines.NProvince["BASE_BUILDING_SLOTS"] + Math.Ceiling(Math.Pow(Population, buildingslots_exponent) * Defines.NProvince["BUILDING_SLOTS_FACTOR"]));

        // province level
        slots += GetModifierValue(ProvinceModifierType.BuildingSlots);
        slots *= 1 + GetModifierValue(ProvinceModifierType.BuildingSlotsFactor);

        // district level
        slots *= 1 + District.GetModifierValue(DistrictModifierType.BuildingSlotsFactor);
        BuildingSlots = (int)slots;

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
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var staticmodifier in StaticModifiers)
        {
            foreach (var modifiernode in staticmodifier.BaseStaticModifiersObj.ModifierNodes)
            {
                var value = (double)modifiernode.GetValue(value_executionstate, staticmodifier.ScaleBy);
                UpdateOrAddModifier((ProvinceModifierType)modifiernode.provinceModifierType!, value);
            }
        }

        if (CurrentDevelopmentStage is not null)
        {
            value_executionstate = new ExecutionState(District, this);
            foreach (var modifiernode in CurrentDevelopmentStage.ModifierNodes)
            {
                var value = (double)modifiernode.GetValue(value_executionstate, 1);
                UpdateOrAddModifier((ProvinceModifierType)modifiernode.provinceModifierType!, value);
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
                UpdateOrAddModifier((ProvinceModifierType)modifiernode.provinceModifierType!, value);
            }
        }
    }
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