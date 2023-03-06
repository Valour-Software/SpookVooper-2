using SV2.Database.Managers;
using SV2.Scripting.LuaObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Scripting;
using Valour.Shared;

namespace SV2.Database.Models.Buildings;

public enum BuildingType
{
    Mine = 0,
    Farm = 3,
    Factory = 1,
    Recruitment_Center = 2,
    Infrastructure = 4
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
    public string LuaBuildingObjId { get; set; }
    public string? Description { get; set; }
    public long ProvinceId { get; set; }
    public long OwnerId { get; set; }
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    [NotMapped]
    public Province Province => DBCache.Get<Province>(ProvinceId)!;

    [NotMapped]
    public BaseRecipe Recipe => GameDataManager.BaseRecipeObjs[RecipeId];

    [NotMapped]
    public LuaBuilding BuildingObj => GameDataManager.BaseBuildingObjs[LuaBuildingObjId];

    [NotMapped]
    public District District => DBCache.Get<District>(DistrictId)!;

    public static BuildingBase Find(long? id)
    {
        if (id == null) return null;

        BuildingBase obj = DBCache.Get<Factory>(id)!;
        if (obj is not null) return obj;

        obj = DBCache.Get<Mine>(id)!;
        if (obj is not null) return obj;

        return null;
    }

    public bool SuccessfullyTicked { get; set; }

    public async Task Tick() { }
}

public abstract class ProducingBuilding : BuildingBase
{
    public long? EmployeeId { get; set; }
    public double Quantity { get; set; }

    [NotMapped]
    public double Efficiency
    {
        get
        {
            double eff = 1.0;
            if (BuildingObj.BaseEfficiency is not null)
                eff = (double)BuildingObj.BaseEfficiency.GetValue(new ExecutionState(District, Province));

            if (BuildingType == BuildingType.Factory) {
                eff -= ((Size * Defines.NProduction["FACTORY_INPUT_EFFICIENCY_LOSS_PER_SIZE"]) - Defines.NProduction["FACTORY_INPUT_EFFICIENCY_LOSS_PER_SIZE"]);
                eff += District.GetModifierValue(DistrictModifierType.FactoryEfficiency);
                eff *= 1 + District.GetModifierValue(DistrictModifierType.FactoryEfficiencyFactor);
            }
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
    public double ThroughputFactor
    {
        get
        {
            var basevalue = BuildingType switch
            {
                BuildingType.Farm => 1 + District.GetModifierValue(DistrictModifierType.FarmThroughputFactor),
                BuildingType.Mine => 1 + District.GetModifierValue(DistrictModifierType.MineThroughputFactor),
                BuildingType.Factory => 1 + District.GetModifierValue(DistrictModifierType.FactoryThroughputFactor),
                _ => 0.00
            };
            basevalue *= BuildingType switch
            {
                BuildingType.Farm => 1 + Province.GetModifierValue(ProvinceModifierType.FarmThroughputFactor),
                BuildingType.Mine => 1 + Province.GetModifierValue(ProvinceModifierType.MineThroughputFactor),
                BuildingType.Factory => 1 + Province.GetModifierValue(ProvinceModifierType.FactoryThroughputFactor),
                _ => 0.00
            };
            basevalue *= Province.GetModifierValue(ProvinceModifierType.AllProducingBuildingThroughputFactor) + 1.00;
            basevalue *= District.GetModifierValue(DistrictModifierType.AllProducingBuildingThroughputFactor) + 1.00;
            return basevalue;
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
                BuildingType.Farm => District.GetModifierValue(DistrictModifierType.FarmQuantityCap),
                BuildingType.Mine => District.GetModifierValue(DistrictModifierType.MineQuantityCap),
                BuildingType.Factory => District.GetModifierValue(DistrictModifierType.FactoryQuantityCap),
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

        rate *= ThroughputFactor;
        rate *= Recipe.PerHour;
        return rate;
    }

    public double GetRateForProduction() {
        double rate = 1;

        rate *= Size;

        rate *= Recipe.PerHour;

        rate *= Defines.NProduction[$"BASE_{BuildingType}_THROUGHPUT"];

        rate *= Quantity;

        rate *= ThroughputFactor;

        return rate;
    }

    public double GetHourlyProduction(bool useQuantity = true) {
        return GetProductionSpeed(useQuantity) * Size;
    }

    public double MiningOutputFactor() {
        if (!Province.Metadata.Resources.ContainsKey(BuildingObj.MustHaveResource)) return 0.0;
        return Province.Metadata.Resources[BuildingObj.MustHaveResource]/2550.0 / 3;
    }

    public async ValueTask<TaskResult> TickRecipe(BaseEntity owner) {
        double rate = GetRateForProduction();
        if (!Recipe.Inputcost_Scaleperlevel)
            rate /= Size;
        double rate_for_input = rate * (1/Efficiency);
        SuccessfullyTicked = false;
        foreach (var resourcename in Recipe.Inputs.Keys) {
            double amount = rate_for_input * Recipe.Inputs[resourcename];
            if (!await owner.HasEnoughResource(resourcename, amount))
                return new(false, "Owner lacks enough resources to tick this building");
        }
        foreach (var resourcename in Recipe.Inputs.Keys) {
            double amount = rate_for_input * Recipe.Inputs[resourcename];
            await owner.ChangeResourceAmount(resourcename, -amount);
        }

        // do output handling now
        foreach (var resourcename in Recipe.Outputs.Keys) {
            double amount = rate * Recipe.Outputs[resourcename];
            if (BuildingObj.MustHaveResource is not null)
                amount *= MiningOutputFactor();
            await owner.ChangeResourceAmount(resourcename, amount);
        }

        SuccessfullyTicked = true;

        return new(true, "");
    }

    public double OutputPerHourPerSize(string resource)
    {
        return 0;
        //return Recipe.Outputs.FirstOrDefault(x => x.Key == resource).Value * GetProductionSpeed();
    }
}