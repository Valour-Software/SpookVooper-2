using SV2.Scripting;

namespace SV2.Scripting.LuaObjects;

public class LuaRecipeEdit
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
    public DictNode Costs { get; set; }
}
