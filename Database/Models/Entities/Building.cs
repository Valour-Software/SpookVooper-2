using SV2.Database.Managers;
using SV2.Scripting.LuaObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Buildings;

public enum BuildingType
{
    Mine = 0,
    Farm = 3,
    Factory = 1,
    Recruitment_Center = 2
}

public interface ITickable
{
    public Task Tick();
}

public abstract class BuildingBase : IHasOwner, ITickable
{
    [Key]
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public int Size { get; set; }
    public string RecipeId { get; set; }
    public abstract BuildingType BuildingType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long? CityId { get; set; }
    public long ProvinceId { get; set; }
    public long OwnerId { get; set; }
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    [NotMapped]
    public Province Province => DBCache.Get<Province>(ProvinceId)!;

    [NotMapped]
    public BaseRecipe Recipe => ResourceManager.Recipes[RecipeId];

    [NotMapped]
    public City? City => DBCache.Get<City>(CityId)!;

    [NotMapped]
    public LuaBuilding Building => BuildingManager.BaseBuildingObjs[Name];

    [NotMapped]
    public District District => DBCache.Get<District>(DistrictId)!;

    public async Task Tick() { }
}

public abstract class ProducingBuilding : BuildingBase
{
    public long? EmployeeId { get; set; }
    public double Quantity { get; set; }

    public double Efficiency
    {
        get
        {
            double eff = 1.00 - ((Size * Defines.NProduction["FACTORY_INPUT_EFFICIENCY_LOSS_PER_SIZE"]) - Defines.NProduction["FACTORY_INPUT_EFFICIENCY_LOSS_PER_SIZE"]);
            eff += District.GetModifier(DistrictModifierType.FactoryEfficiency).Amount;
            eff *= 1 + District.GetModifier(DistrictModifierType.FactoryEfficiencyFactor).Amount;
            return eff;
        }
    }

    [NotMapped]
    public double QuantityGrowthRateFactor
    {
        get
        {
            string type = BuildingType.ToString().ToUpper();
            return Defines.NProduction[$"BASE_{type}_QUANTITY_GROWTH_RATE_FACTOR"];
        }
    }

    [NotMapped]
    public double ProductionFactor
    {
        get
        {
            var basevalue = BuildingType switch
            {
                BuildingType.Farm => 1 + District.GetModifier(DistrictModifierType.FarmProductionFactor).Amount,
                BuildingType.Mine => 1 + District.GetModifier(DistrictModifierType.MineProductionFactor).Amount,
                BuildingType.Factory => 1 + District.GetModifier(DistrictModifierType.FactoryProductionFactor).Amount,
                _ => 0.00
            };
            return basevalue * (District.GetModifier(DistrictModifierType.AllProducingBuildingThroughputFactor).Amount + 1.00);
        }
    }

    [NotMapped]
    public double QuantityCap
    {
        get
        {
            string type = BuildingType.ToString().ToUpper();
            return Defines.NProduction[$"BASE_{type}_QUANTITY_CAP"] + BuildingType switch
            {
                BuildingType.Farm => District.GetModifier(DistrictModifierType.FarmQuantityCap).Amount,
                BuildingType.Mine => District.GetModifier(DistrictModifierType.MineQuantityCap).Amount,
                BuildingType.Factory => District.GetModifier(DistrictModifierType.FactoryQuantityCap).Amount,
                _ => 0.00
            };
        }
    }

    public double GetProductionSpeed(bool useQuantity = true)
    {
        string type = BuildingType.ToString().ToUpper();
        double rate = Defines.NProduction[$"BASE_{type}_THROUGHPUT"];
        if (useQuantity)
            rate *= Quantity;

        rate *= ProductionFactor;
        //rate *= Recipe.Perhour;
        return rate;
    }

    public double OutputPerHourPerSize(string resource)
    {
        return 0;
        //return Recipe.Outputs.FirstOrDefault(x => x.Key == resource).Value * GetProductionSpeed();
    }
}