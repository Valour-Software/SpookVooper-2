using SV2.Database.Managers;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Districts;

public class City
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long DistrictId { get; set; }

    [ForeignKey(nameof(DistrictId))]
    public District District { get; set; }

    public long Population { get; set; }

    public int BuildingSlots { get; set; }

    [NotMapped]
    // this is generated upon server start
    public List<DistrictModifier> Modifiers { get; set; }

    public DistrictModifier GetModifier(DistrictModifierType type) => Modifiers.First(x => x.ModifierType == type);

    public async Task HourlyTick()
    {
        double BirthRate = Defines.NCity["BASE_BIRTH_RATE"];
        BirthRate += GetModifier(DistrictModifierType.MonthlyBirthRate).Amount;
        BirthRate *= GetModifier(DistrictModifierType.MonthlyBirthRateFactor).Amount + 1;

        double DeathRate = Defines.NCity["BASE_DEATH_RATE"];
        DeathRate += GetModifier(DistrictModifierType.MonthlyDeathRate).Amount;
        DeathRate *= GetModifier(DistrictModifierType.MonthlyDeathRateFactor).Amount + 1;

        double PopulationGrowth = BirthRate * Population;
        PopulationGrowth -= DeathRate * Population;
        PopulationGrowth -= Math.Pow(Population, Defines.NCity[NCity.OVERPOPULATION_MODIFIER_EXPONENT]);

        // get hourly rate
        PopulationGrowth = PopulationGrowth / 30 / 24;
        Population += (long)Math.Ceiling(PopulationGrowth);
    }
}
