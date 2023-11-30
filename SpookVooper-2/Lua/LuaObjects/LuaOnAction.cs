using SV2.Scripting;

namespace SV2.Scripting.LuaObjects;

public enum OnActionType {
    OnServerStart
}
public class LuaOnAction {
    public OnActionType OnActionType { get; set; }
    public EffectBody EffectBody { get; set; }
}