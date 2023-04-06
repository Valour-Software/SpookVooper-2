using SV2.Database.Managers;
using SV2.Scripting.LuaObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Scripting;
using Valour.Shared;
using Valour.Api.Models;

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
    public ValueTask Tick();
}

public abstract class BuildingBase : IHasOwner, ITickable
{
    [Key]
    public long Id { get; set; }
    public string? Name { get; set; }
    public long DistrictId { get; set; }
    public int Size { get; set; }
    public string RecipeId { get; set; }
    public abstract BuildingType BuildingType { get; }
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

    public virtual async ValueTask Tick() { }
}

public abstract class ProducingBuilding : BuildingBase
{
    public ProducingBuilding() {

    }
    public long? EmployeeId { get; set; }
    public double Quantity { get; set; }

    [NotMapped]
    public double QuantityHourlyGrowth {
        get {
            double quantitychange = Defines.NProduction["BASE_QUANTITY_GROWTH_RATE"] / 24;
            quantitychange *= (QuantityCap * QuantityCap) / Quantity;
            return quantitychange * QuantityGrowthRateFactor;
        }
    }

    [NotMapped]
    public double Efficiency
    {
        get
        {
            double eff = 1.0;
            if (BuildingObj.BaseEfficiency is not null)
                eff = (double)BuildingObj.BaseEfficiency.GetValue(new ExecutionState(District, Province));

            if (BuildingType == BuildingType.Factory) {
                eff -= (((Size - 1) * Defines.NProduction["FACTORY_INPUT_EFFICIENCY_LOSS_PER_SIZE"]) - Defines.NProduction["FACTORY_INPUT_EFFICIENCY_LOSS_PER_SIZE"]);
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
                _ => 1
            };
            basevalue *= BuildingType switch
            {
                BuildingType.Farm => 1 + Province.GetModifierValue(ProvinceModifierType.FarmThroughputFactor),
                BuildingType.Mine => 1 + Province.GetModifierValue(ProvinceModifierType.MineThroughputFactor),
                BuildingType.Factory => 1 + Province.GetModifierValue(ProvinceModifierType.FactoryThroughputFactor),
                _ => 1
            };

            if (BuildingObj.ApplyStackingBonus)
                basevalue += Math.Min(Defines.NProduction["STACKING_THROUGHPUT_BONUS"] * Size, Defines.NProduction["MAX_STACKING_THROUGHPUT_BONUS"]);

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
                _ => 1
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

        rate *= Defines.NProduction[$"BASE_{BuildingType.ToString().ToUpper()}_THROUGHPUT"];

        rate *= Quantity;

        rate *= ThroughputFactor;

        return rate;
    }

    public double GetHourlyProduction(bool useQuantity = true) {
        return GetProductionSpeed(useQuantity) * Size;
    }

    public double MiningOutputFactor() {
        if (!Province.Metadata.Resources.ContainsKey(BuildingObj.MustHaveResource)) return 0.0;
        return Province.Metadata.Resources[BuildingObj.MustHaveResource]/10550.0;
    }

    public async ValueTask<TaskResult> TickRecipe() {
        double rate = GetRateForProduction();
        if (!Recipe.Inputcost_Scaleperlevel)
            rate /= Size;
        double rate_for_input = rate * (1/Efficiency);
        SuccessfullyTicked = false;
        foreach (var resourcename in Recipe.Inputs.Keys) {
            double amount = rate_for_input * Recipe.Inputs[resourcename];
            if (!await Owner.HasEnoughResource(resourcename, amount))
                return new(false, "Owner lacks enough resources to tick this building");
        }
        foreach (var resourcename in Recipe.Inputs.Keys) {
            double amount = rate_for_input * Recipe.Inputs[resourcename];
            await Owner.ChangeResourceAmount(resourcename, -amount, $"Input for building {Name} ({BuildingObj.PrintableName})");
        }

        // do output handling now
        foreach (var resourcename in Recipe.Outputs.Keys) {
            double amount = rate * Recipe.Outputs[resourcename];
            if (BuildingObj.MustHaveResource is not null)
                amount *= MiningOutputFactor();
            await Owner.ChangeResourceAmount(resourcename, amount, $"Output for building {Name} ({BuildingObj.PrintableName})");
        }

        SuccessfullyTicked = true;

        return new(true, "");
    }

    public double OutputPerHourPerSize(string resource)
    {
        return 0;
        //return Recipe.Outputs.FirstOrDefault(x => x.Key == resource).Value * GetProductionSpeed();
    }

    public bool CanManage(BaseEntity entity)
    {
        if (OwnerId == entity.Id || (Owner.EntityType != EntityType.User && ((Group)Owner).HasPermission(entity, GroupPermissions.ManageBuildings)))
            return true;
        return false;
    }
}