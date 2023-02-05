using SV2.Scripting.Parser;

namespace SV2.Scripting.LuaObjects;


public class LuaBuilding
{
    public string Name { get; set; }
    public long LandUsage { get; set; }
    public Dictionary<string, decimal> BuildingCosts { get; set; }
    public BuildingType type { get; set; }
    public List<BaseRecipe> Recipes { get; set; }
    public string PrintableName => Name.Replace("building_", "").Replace("_", " ").ToTitleCase();
}