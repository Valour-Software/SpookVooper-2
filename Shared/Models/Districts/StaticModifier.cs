namespace Shared.Models.Districts;
public class StaticModifier
{
    public bool Decay { get; set; }
    public DateTime StartDate { get; set; }

    /// <summary>
    /// In hours (ticks)
    /// </summary>
    public int? Duration { get; set; }
    public decimal ScaleBy { get; set; }
    public string LuaStaticModifierObjId { get; set; }

    public LuaStaticModifier BaseStaticModifiersObj => SVCache.Get<LuaStaticModifier>(LuaStaticModifierObjId);
}