using Shared.Lua;
using Shared.Lua.Scripting;
using Shared.Models.Districts;

namespace Shared.Lua.LuaObjects;

public enum BuildingType
{
    Mine = 0,
    Farm = 3,
    Factory = 1,
    Recruitment_Center = 2,
    Infrastructure = 4,
    ResearchLab = 5
}

public class LuaBuilding : Item
{
    public override string BaseRoute => "api/lua/luabuildings";
    public string Name { get; set; }
    public DictNode BuildingCosts { get; set; }
    public BuildingType type { get; set; }
    //public List<BaseRecipe> Recipes { get; set; }
    public string PrintableName => Name.Replace("building_", "").Replace("_", " ").ToTitleCase();
    public bool OnlyGovernorCanBuild { get; set; }
    public ExpressionNode? BaseEfficiency { get; set; }
    public bool UseBuildingSlots { get; set; }
    public string MustHaveResource { get; set; }
    public bool ApplyStackingBonus { get; set; }

    public static async ValueTask<LuaBuilding> FindAsync(string id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<LuaBuilding>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<LuaBuilding>($"api/lua/luabuildings/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Name, this);
    }
}