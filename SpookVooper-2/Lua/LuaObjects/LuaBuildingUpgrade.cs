using SV2.Database.Managers;
using SV2.Scripting;
using Valour.Shared;

namespace SV2.Scripting.LuaObjects;

public class LuaBuildingUpgrade
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DictNode Costs { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }

    public Dictionary<string, double> GetConstructionCost(BaseEntity entity, District district, Province province, ProducingBuilding? building, BuildingUpgrade? upgrade, int levels)
    {
        Dictionary<string, double> totalresources = new();
        Dictionary<string, decimal> changesystemvarsby = new Dictionary<string, decimal>() {
            { @"province.buildings.totaloftype[""infrastructure""]", 0.0m },
            { "upgrade.level", (decimal)(upgrade is not null ? 0-upgrade.Level : 0) }
        };
        for (int i = 0; i < levels; i++)
        {
            var costs = Costs.Evaluate(new ExecutionState(district, province, changesystemvarsby, building: building, buildingUpgrade: upgrade is null ? new() : upgrade));
            foreach ((var resource, var amount) in costs)
            {
                if (!totalresources.ContainsKey(resource))
                    totalresources[resource] = 0;
                if (building is not null)
                    totalresources[resource] += (double)amount*building.Size;
                else
                    totalresources[resource] += (double)amount;
            }

            changesystemvarsby["upgrade.level"] += 1.0m;
        }
        return totalresources;
    }

    public async ValueTask<TaskResult> CanBuild(BaseEntity buildas, BaseEntity caller, District district, Province province, ProducingBuilding building, BuildingUpgrade? upgrade, int levels)
    {
        if (levels <= 0)
            return new(false, "The amount of levels you wish to upgrade must be greater than 0!");

        var costs = GetConstructionCost(buildas, district, province, building, upgrade, levels);

        // check for resources
        foreach ((var resource, var amount) in costs)
        {
            if (!await buildas.HasEnoughResource(resource, amount))
                return new(false, $"{buildas.Name} lack enough {resource}! About {(amount - (await buildas.GetOwnershipOfResource(resource))):n0} more is required");
        }

        return new(true, null);
    }

    public async ValueTask<TaskResult<ProducingBuilding>> Build(BaseEntity buildas, BaseEntity caller, District district, Province province, int levels, ProducingBuilding building, BuildingUpgrade? upgrade)
    {
        var canbuild = await CanBuild(buildas, caller, district, province, building, upgrade, levels);
        if (!canbuild.Success)
            return new(false, canbuild.Message);

        var costs = GetConstructionCost(buildas, district, province, building, upgrade, levels);
        foreach ((var resource, var amount) in costs)
            await buildas.ChangeResourceAmount(resource, -amount, "Construction");

        if (upgrade is null)
        {
            upgrade = new()
            {
                LuaBuildingUpgradeId = Id,
                Level = levels
            };
            building.Upgrades.Add(upgrade);
        }
        else
        {
            upgrade.Level += levels;
        }

        district.UpdateModifiers();

        return new(true, $"Successfully upgraded {Name} {levels} time(s).", building);
    }
}
