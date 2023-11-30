using Shared.Lua.Scripting;

namespace Shared.Lua.LuaObjects;

public enum OnActionType
{
    OnServerStart
}
public class LuaOnAction
{
    public OnActionType OnActionType { get; set; }
    public EffectBody EffectBody { get; set; }
}