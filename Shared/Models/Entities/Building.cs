using Shared.Models.Districts;
using System.Text.Json.Serialization;
using Shared.Managers;

namespace Shared.Models.Buildings;

public interface ITickable
{
    public ValueTask Tick();
}

public abstract class BuildingBase : Item, ITickable
{
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

    public async ValueTask<BaseEntity> GetOwnerAsync() => await BaseEntity.FindAsync(OwnerId);
    public async ValueTask<Province> GetProvinceAsync() => await Province.FindAsync(ProvinceId);
    public async ValueTask<LuaBuilding> GetLuaBuildingAsync() => await LuaBuilding.FindAsync(LuaBuildingObjId);
    public async ValueTask<District> GetDistrictAsync() => await District.FindAsync(DistrictId);
    public async ValueTask<Recipe> GetRecipeAsync() => await Recipe.FindAsync(RecipeId);

    public bool SuccessfullyTicked { get; set; }

    public virtual async ValueTask Tick() { }
}

public class BuildingUpgrade
{
    public string LuaBuildingUpgradeId { get; set; }

    public async ValueTask<LuaBuildingUpgrade> GetLuaBuildingUpgradeAsync() => await LuaBuildingUpgrade.FindAsync(LuaBuildingUpgradeId);

    public int Level { get; set; }
}

[JsonDerivedType(typeof(Factory), typeDiscriminator: 1)]
[JsonDerivedType(typeof(Mine), typeDiscriminator: 2)]
[JsonDerivedType(typeof(Farm), typeDiscriminator: 3)]
[JsonDerivedType(typeof(Infrastructure), typeDiscriminator: 4)]
public abstract class ProducingBuilding : BuildingBase
{
    public long? EmployeeId { get; set; }
    public double Quantity { get; set; }

    public Dictionary<BuildingModifierType, double> Modifiers { get; set; }

    public List<BuildingUpgrade>? Upgrades { get; set; }

    public List<StaticModifier>? StaticModifiers { get; set; }

    public double QuantityHourlyGrowth => 1.0;
    public double QuantityCap => 1.0;

    public static async ValueTask<ProducingBuilding> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<ProducingBuilding>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<ProducingBuilding>($"api/buildings/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }

    public BaseEntity Owner { get; set; }
    public District District { get; set; }
    public Province Province { get; set; }
    public Recipe Recipe { get; set; }
}

public enum BuildingModifierType
{
    ThroughputFactor,
    EfficiencyFactor
}