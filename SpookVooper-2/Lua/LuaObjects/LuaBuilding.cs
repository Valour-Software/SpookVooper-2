using IdGen;
using SV2.Database.Managers;
using SV2.Managers;
using SV2.Scripting;
using SV2.Scripting.Parser;
using System.Text.Json.Serialization;
using Valour.Shared;

namespace SV2.Scripting.LuaObjects;


public class LuaBuilding
{
    public string Name { get; set; }
    public DictNode BuildingCosts { get; set; }
    public BuildingType type { get; set; }

    [JsonIgnore]
    public List<BaseRecipe> Recipes { get; set; }
    public List<LuaBuildingUpgrade> Upgrades { get; set; }
    public string PrintableName => Name.Replace("building_", "").Replace("_", " ").ToTitleCase();
    public bool OnlyGovernorCanBuild { get; set; }
    public ExpressionNode? BaseEfficiency { get; set; }
    public bool UseBuildingSlots { get; set; }
    public string MustHaveResource { get; set; }
    public bool ApplyStackingBonus { get; set; }

    public Dictionary<string, double> GetConstructionCost(BaseEntity entity, District district, Province province, ProducingBuilding? building, int levels) {
        Dictionary<string, double> totalresources = new();
        Dictionary<string, decimal> changesystemvarsby = new Dictionary<string, decimal>() {
            { @"province.buildings.totaloftype[""infrastructure""]", 0.0m },
            { "upgrade.level", 0.0m }
        };

        // do we have upgrades
        if (building is not null && building.Upgrades.Count > 0)
        {
            // cry a bit
            // get total cost of resources for the upgrades
            foreach (var upgrade in building.Upgrades)
            {
                foreach ((var resource, var amount) in upgrade.LuaBuildingUpgradeObj.GetConstructionCost(entity, district, province, building, upgrade, upgrade.Level))
                {
                    if (!totalresources.ContainsKey(resource))
                        totalresources[resource] = 0;
                    totalresources[resource] += ((double)amount) * levels;
                }
            }
        }

        for (int i = 0; i < levels; i++) {
            var costs = BuildingCosts.Evaluate(new ExecutionState(district, province, changesystemvarsby));
            foreach ((var resource, var amount) in costs) {
                if (!totalresources.ContainsKey(resource))
                    totalresources[resource] = 0;
                totalresources[resource] += (double)amount;
            }

            if (Name == "building_infrastructure")
                changesystemvarsby["province.buildings.totaloftype[\"infrastructure\"]"] += 1.0m;
        }
        return totalresources;
    }

    public async ValueTask<TaskResult> CanBuild(BaseEntity buildas, BaseEntity caller, District district, Province province, ProducingBuilding? building, int levels) {
        if (levels <= 0)
            return new(false, "The amount of levels you wish to build must be greater than 0!");

        if (OnlyGovernorCanBuild && !province.CanManageBuildingRequests(caller))
            return new(false, $"Only the Governor of {province.Name} can build this building!");

        var costs = GetConstructionCost(buildas, district, province, building, levels);

        // check for resources
        foreach ((var resource, var amount) in costs) {
            if (!await buildas.HasEnoughResource(resource, amount)) {
                return new(false, $"{buildas.Name} lack enough {resource}! About {(amount - (await buildas.GetOwnershipOfResource(resource))):n0} more is required");
            }
        }

        // check for building slots
        int slotsleftover = province.BuildingSlots - (province.BuildingSlotsUsed + levels);
        if (slotsleftover < 0 && UseBuildingSlots)
            return new(false, $"{province.Name} lacks enough building slots! {slotsleftover} more building slots are required!");

        return new(true, null);
    }

    public async ValueTask<TaskResult<ProducingBuilding>> Build(BaseEntity buildas, BaseEntity caller, District district, Province province, int levels, ProducingBuilding? building = null) {
        var canbuild = await CanBuild(buildas, caller, district, province, building, levels);
        if (!canbuild.Success)
            return new(false, canbuild.Message);

        var costs = GetConstructionCost(buildas, district, province, building, levels);
        foreach ((var resource, var amount) in costs) {
            await buildas.ChangeResourceAmount(resource, -amount, "Construction");
        }

        if (building is null) {
            building = type switch {
                BuildingType.Mine => new Mine(),
                BuildingType.Factory => new Factory(),
                BuildingType.Farm => new Farm(),
                BuildingType.Infrastructure => new Infrastructure()
            };
            building.StaticModifiers = new();
            building.Modifiers = new();
            building.Id = IdManagers.GeneralIdGenerator.Generate();
            building.OwnerId = buildas.Id;
            building.DistrictId = district.Id;
            building.ProvinceId = province.Id;
            building.RecipeId = Recipes.First().Id;
            building.LuaBuildingObjId = Name;
            building.Size = levels;
            building.Name = IdManagers.GeneralIdGenerator.Generate().ToString();
            switch (type) {
                case BuildingType.Mine:
                    building.Quantity = Defines.NProduction["BASE_MINE_QUANTITY"];
                    var mine = (Mine)building;
                    DBCache.Put(mine.Id, mine);
                    DBCache.ProvincesBuildings[province.Id].Add(mine);
                    DBCache.dbctx.Mines.Add(mine);
                    break;
                case BuildingType.Factory:
                    building.Quantity = Defines.NProduction["BASE_FACTORY_QUANTITY"];
                    var factory = (Factory)building;
                    DBCache.Put(factory.Id, factory);
                    DBCache.ProvincesBuildings[province.Id].Add(factory);
                    DBCache.dbctx.Factories.Add(factory);
                    break;
                case BuildingType.Farm:
                    building.Quantity = Defines.NProduction["BASE_FARM_QUANTITY"];
                    var farm = (Farm)building;
                    DBCache.Put(farm.Id, farm);
                    DBCache.ProvincesBuildings[province.Id].Add(farm);
                    DBCache.dbctx.Farms.Add(farm);
                    break;
                case BuildingType.Infrastructure:
                    building.Quantity = 1;
                    var infrastructure = (Infrastructure)building;
                    DBCache.Put(infrastructure.Id, infrastructure);
                    DBCache.ProvincesBuildings[province.Id].Add(infrastructure);
                    DBCache.dbctx.Infrastructures.Add(infrastructure);
                    await building.Tick();
                    await building.TickRecipe();
                    province.UpdateModifiers();
                    province.UpdateModifiersAfterBuildingTick();
                    break;
            }
            building.Quantity = 1;
        }

        else {
            building.Size += levels;
            if (building.BuildingType == BuildingType.Infrastructure) {
                await building.Tick();
                await building.TickRecipe();
                province.UpdateModifiers();
                province.UpdateModifiersAfterBuildingTick();
            }
        }

        district.UpdateModifiers();

        return new(true, $"Successfully built {levels} levels of {PrintableName}.", building);
    }
}