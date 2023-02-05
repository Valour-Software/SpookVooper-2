namespace SV2.Database.Models.Districts.Modifiers;

/// <summary>
/// Enum of all modifiers in the District scope
/// "Factor" means a % effect, if something does not have "Factor" in its name then it's just adding the modifier
/// </summary>
public enum DistrictModifierType 
{
    MiningProductionFactor,
    SmeltingEfficiency,
    MonthlyBirthRate,
    MonthlyDeathRate,
    MonthlyBirthRateFactor,
    MonthlyDeathRateFactor,
    MineQuantityCap,
    MineQuantityGrowthRateFactor,
    MineProductionFactor,
    FarmQuantityCap,
    FarmQuantityGrowthRateFactor,
    FarmProductionFactor,
    FactoryQuantityCap,
    FactoryQuantityGrowthRateFactor,
    FactoryProductionFactor,
    FactoryEfficiencyFactor,
    FactoryEfficiency,
    PopulationGrowthFactor,
    ArmyAttackFactory,
    ArmyEntrenchmentFactor,
    ArmyEntrenchment,
    ArmyEntrenchmentSpeed,
    ArmyEntrenchmentSpeedFactor,
    ArmySpeedFactor,
    ArmyMorale,
    ArmyMoraleFactor,
    DivisionXpGainFactor,
    RecruitmentCenterManpowerFactor,
    AllProducingBuildingThroughputFactor
}