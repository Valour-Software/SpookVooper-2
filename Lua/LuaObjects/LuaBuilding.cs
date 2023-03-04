using SV2.Scripting;
using SV2.Scripting.Parser;

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
}