using Shared.Lua;
using Shared.Lua.Scripting;

namespace Shared.Lua.LuaObjects;

public class LuaPolicyOption
{
    public string Id { get; set; }
    public string Name => Id.Replace("_", " ").ToTitleCase();
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
}

public enum LuaPolicyType
{
    Province,
    State,
    District,
    Imperial
}

public class LuaPolicy
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DefaultOption { get; set; }
    public LuaPolicyType Type { get; set; }
    public Dictionary<string, LuaPolicyOption> Options { get; set; }
}