using SV2.Scripting.LuaObjects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SV2.Database.Models.Districts;
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

    [NotMapped]
    [JsonIgnore]
    public LuaStaticModifier BaseStaticModifiersObj => GameDataManager.BaseStaticModifiersObjs[LuaStaticModifierObjId];
}