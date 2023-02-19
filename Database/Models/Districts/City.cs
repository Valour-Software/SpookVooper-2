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

    public long Population { get; set; }

    public bool IsCapitalCity { get; set; }

    public int BuildingSlots { get; set; }

    //[NotMapped]
    // this is generated upon server start
    //public List<DistrictModifier> Modifiers { get; set; }

    //public DistrictModifier GetModifier(DistrictModifierType type) => Modifiers.First(x => x.ModifierType == type);

    public async Task HourlyTick()
    {
        double BirthRate = Defines.NCity["BASE_BIRTH_RATE"];
        BirthRate += District.GetModifier(DistrictModifierType.MonthlyBirthRate).Amount;
        BirthRate *= District.GetModifier(DistrictModifierType.MonthlyBirthRateFactor).Amount + 1;

        double DeathRate = Defines.NCity["BASE_DEATH_RATE"];
        DeathRate += District.GetModifier(DistrictModifierType.MonthlyDeathRate).Amount;
        DeathRate *= District.GetModifier(DistrictModifierType.MonthlyDeathRateFactor).Amount + 1;

        double PopulationGrowth = BirthRate * Population;
        PopulationGrowth -= DeathRate * Population;
        PopulationGrowth -= Math.Pow(Population, Defines.NCity[NCity.OVERPOPULATION_MODIFIER_EXPONENT]);

        // get hourly rate
        PopulationGrowth = PopulationGrowth / 30 / 24;
        Population += (long)Math.Ceiling(PopulationGrowth);

        // update building slot count
        BuildingSlots = (int)(Defines.NCity["BASE_BUILDING_SLOTS"] + Math.Ceiling((Math.Pow(Population, Defines.NCity["BUILDING_SLOTS_POPULATION_EXPONENT"]) / Defines.NCity["BUILDING_SLOTS_FACTOR"])));
    }
}
