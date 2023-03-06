using IdGen;
using SV2.Database.Managers;
using SV2.Managers;
using SV2.NonDBO;
using SV2.Scripting;
using SV2.Scripting.Parser;
using Valour.Shared;

namespace SV2.Scripting.LuaObjects;


public class LuaBuilding
{
    public string Name { get; set; }
    public DictNode BuildingCosts { get; set; }
    public BuildingType type { get; set; }
    public List<BaseRecipe> Recipes { get; set; }
    public string PrintableName => Name.Replace("building_", "").Replace("_", " ").ToTitleCase();
    public bool OnlyGovernorCanBuild { get; set; }
    public ExpressionNode? BaseEfficiency { get; set; }
    public bool UseBuildingSlots { get; set; }
    public string MustHaveResource { get; set; }

    public Dictionary<string, decimal> GetConstructionCost(BaseEntity entity, District district, Province province, int levels) {
        Dictionary<string, decimal> totalresources = new();
        for (int i = 0; i < levels; i++) {
            var costs = BuildingCosts.Evaluate(new ExecutionState(district, province));
            foreach ((var resource, var amount) in costs) {
                if (!totalresources.ContainsKey(resource))
                    totalresources[resource] = 0;
                totalresources[resource] += amount;
            }
        }
        return totalresources;
    }

    public async ValueTask<TaskResult<bool>> CanBuild(BaseEntity entity, District district, Province province, int levels) {
        if (levels <= 0)
            return new(false, "The amount of levels you wish to build must be greater than 0!");
        
        var costs = GetConstructionCost(entity, district, province, levels);

        // check for resources
        foreach ((var resource, var amount) in costs) {
            if (!await entity.HasEnoughResource(resource, amount)) {
                return new(false, $"{entity.Name}'s lack enough {resource}! About {(amount - (await entity.GetOwnershipOfResource(resource))):0n}");
            }
        }

        // check for building slots
        int slotsleftover = province.BuildingSlots - (province.BuildingSlotsUsed + levels);
        if (slotsleftover < 0)
            return new(false, $"{province.Name} lacks enough building slots! {slotsleftover} more building slots are required!");

        if (OnlyGovernorCanBuild && !province.CanManageBuildingRequests(entity))
            return new(false, $"Only the Governor of {province.Name} can build this building!");

        return new(true, null);
    }

    public async ValueTask<TaskResult<bool>> Build(BaseEntity entity, District district, Province province, int levels) {
        var canbuild = await CanBuild(entity, district, province, levels);
        if (!canbuild.Success)
            return new(false, canbuild.Message);

        var costs = GetConstructionCost(entity, district, province, levels);
        foreach ((var resource, var amount) in costs) {
            await entity.ChangeResourceAmount(resource, (int)(Math.Ceiling(amount)));
        }

        ProducingBuilding? building = DBCache.GetAllProducingBuildings().FirstOrDefault(x => x.OwnerId == entity.Id && x.ProvinceId == province.Id && x.LuaBuildingObjId == Name);
        if (building is null) {
            building.Id = IdManagers.GeneralIdGenerator.Generate();
            building.OwnerId = building.OwnerId;
            building.DistrictId = district.Id;
            building.ProvinceId = province.Id;
            building.RecipeId = Recipes.First().Id;
            building.LuaBuildingObjId = Name;
            building.Size = levels;
            switch (type) {
                case BuildingType.Mine:
                    building.Quantity = Defines.NProduction["BASE_MINE_QUANTITY"];
                    var _building = (Mine)building;
                    DBCache.Put(_building.Id, _building);
                    DBCache.dbctx.Mines.Add(_building);
                    break;
            }

            
        }
    }
}