using Shared.Lua.Scripting;

namespace Shared.Lua.LuaObjects;

public class LuaBuildingUpgrade
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DictNode Costs { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
}
