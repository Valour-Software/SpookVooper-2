using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using SV2.Scripting.LuaObjects;

namespace SV2.Database.Models.Districts;

public class DistrictStaticModifier
{
    [Key]
    public long Id { get; set; }
    public long DistrictId { get; set; }

    [ForeignKey(nameof(DistrictId))]
    public District District { get; set; }
    public bool Decay { get; set; } = false;
    public DateTime StartDate { get; set; }
    public int Duration { get; set; } = 0;

    [DecimalType(6)]
    public decimal ScaleBy { get; set; } = 1.00m;
    public string StaticModifierId { get; set; }

    [NotMapped]
    public LuaStaticModifier LuaStaticModifier => StaticModifierManager.BaseStaticModifers[StaticModifierId];

    public void SetModifers(District district)
    {
        foreach (var modifiernode in LuaStaticModifier.ModifierNodes)
        {
            var modifer = district.GetModifier(modifiernode.ModifierType);
            modifer.Amount += (double)modifiernode.GetValue(district, ScaleBy);
        }
    }
}