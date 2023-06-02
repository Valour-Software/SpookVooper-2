using Shared.Lua.Scripting;

namespace Shared.Lua.LuaObjects;

public class LuaStaticModifier
{
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required string Icon { get; set; }
    public required bool Stackable { get; set; }
    public required bool IsGood { get; set; }
    public EffectBody? EffectBody { get; set; }
    public required List<SyntaxModifierNode> ModifierNodes { get; set; }
}