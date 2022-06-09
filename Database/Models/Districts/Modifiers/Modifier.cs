namespace SV2.Database.Models.Districts.Modifiers;

/// <summary>
/// Enum of all modifiers in the District scope
/// "Factor" means a % effect, if something does not have "Factor" in its name then it's just adding the modifier
/// </summary>
public enum DistrictModifierType 
{
    FactorySpeedFactor = 1,
    FactoryBaseQuantity = 2,
    FactoryQuantityGrowthRateFactor = 3,
    FactoryQuantityCap = 4,
    FactoryEfficiencyFactory = 5,
    MineSpeedFactor = 6,
    MineBaseQuantity = 7,
    MineQuantityGrowthRateFactor = 8,
    MineQuantityCap = 9,
    PopulationGrowthFactor = 10,
    ArmyAttackFactory = 11,
    ArmyEntrenchmentFactor = 12,
    ArmyEntrenchment = 13,
    ArmyEntrenchmentSpeed = 14,
    ArmyEntrenchmentSpeedFactor = 15,
    ArmySpeedFactor = 16,
    ArmyMorale = 17,
    ArmyMoraleFactor = 18,
    DivisionXpGainFactor = 19,
    RecruitmentCenterManpowerFactor = 20
}