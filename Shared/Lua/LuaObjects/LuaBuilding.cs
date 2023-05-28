using Shared.Lua;
using Shared.Lua.Scripting;

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

public class LuaBuilding
{
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
}