using SV2.Scripting;

namespace SV2.NonDBO;

public class LuaProvinceStaticModifier
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Stackable { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
}
