using SV2.Scripting;

namespace SV2.Scripting.LuaObjects;

public class LuaBuildingUpgrade
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DictNode Costs { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
}
