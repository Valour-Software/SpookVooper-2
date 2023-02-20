using SV2.Database.Managers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Districts;

public class City
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }
    public long DistrictId { get; set; }

    [NotMapped]
    public District District => DBCache.Get<District>(DistrictId);

    public long ProvinceId { get; set; }

    [NotMapped]
    public Province Province => DBCache.Get<Province>(ProvinceId);

    public bool IsCapitalCity { get; set; }

    //[NotMapped]
    // this is generated upon server start
    //public List<DistrictModifier> Modifiers { get; set; }

    //public DistrictModifier GetModifier(DistrictModifierType type) => Modifiers.First(x => x.ModifierType == type);

    
}
