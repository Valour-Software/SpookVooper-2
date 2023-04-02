using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Managers;
using SV2.Scripting;

namespace SV2.Database.Models.Districts;

public class DistrictModifier
{
    public DistrictModifierType ModifierType { get; set; }
    public double Amount { get; set; }  
}

public class District
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name", TypeName = "VARCHAR(64)")]
    public string? Name { get; set; }

    [NotMapped]
    public string ScriptName => Name.ToLower().Replace(" ", "_");

    [Column("description", TypeName = "VARCHAR(512)")]
    public string? Description { get; set; }

    [NotMapped]
    public List<Province> Provinces { get; set; }

    [NotMapped]
    public List<City> Cities => DBCache.GetAll<City>().Where(x => x.DistrictId == Id).ToList();

    [NotMapped]
    public List<SVUser> Citizens => DBCache.GetAll<SVUser>().Where(x => x.DistrictId == Id).ToList();

    [NotMapped]
    public List<State> States => DBCache.GetAll<State>().Where(x => x.DistrictId == Id).ToList();

    [NotMapped]
    public long TotalPopulation
    {
        get
        {
            if (Provinces is not null)
                return Provinces.Sum(x => x.Population);
            return 0;
        }
    }

    [NotMapped]
    public Group Group => DBCache.Get<Group>(GroupId)!;

    [Column("groupid")]
    public long GroupId { get; set; }

    [NotMapped]
    public Senator? Senator => DBCache.Get<Senator>(Id);

    [Column("governorid")]
    public long? GovernorId { get; set; }

    [NotMapped]
    public SVUser? Governor => DBCache.Get<SVUser>(GovernorId);

    [Column("flagurl", TypeName = "VARCHAR(256)")]
    public string? FlagUrl { get; set; }

    [Column("basepropertytax")]
    public double? BasePropertyTax { get; set; }

    [Column("propertytaxpersize")]
    public double? PropertyTaxPerSize { get; set; }

    [Column("staticmodifiers", TypeName = "jsonb[]")]
    public List<StaticModifier> StaticModifiers { get; set; }

    [NotMapped]
    public Dictionary<DistrictModifierType, DistrictModifier> Modifiers = new();

    [NotMapped]
    public double EconomicScore { get; set; }

    [NotMapped]
    public string Color => Name switch
    {
        "Lanatia" => "F4B7FD",
        "New Vooperis" => "FEEAB7",
        "Elysian Katonia" => "B8B7FD",
        "Ardenti Terra" => "B7BCFC",
        "Landing Cove" => "FDB7B7",
        "New Avalon" => "D3FCB6",
        "Novastella" => "B7FDE5",
        "Old King" => "C0FDB7",
        "San Vooperisco" => "FAFDB8",
        "Thesonica" => "FDD9B7",
        "Voopmont" => "FFFFFF",
    };

    public static District Find(long id)
    {
        return DBCache.GetAll<District>().FirstOrDefault(x => x.Id == id)!;
    }

    public double GetModifierValue(DistrictModifierType modifierType)
    {
        if (!Modifiers.ContainsKey(modifierType))
            return 0;
        return Modifiers[modifierType].Amount;
    }

    [NotMapped]
    public List<Province> ProvincesByDevelopmnet { get; set; }

    [NotMapped]
    public List<Province> ProvincesByMigrationAttraction { get; set; }

    public void HourlyTick()
    {
        double totalattractionpoints = Provinces.Sum(x => Math.Pow(x.MigrationAttraction, 1.025));

        // do migration
        // this was very "fun" to code
        //double totalmigration = Provinces.Sum(x => x.Population) * Defines.NProvince[NProvince.BASE_MIGRATION_RATE] / 30 / 24;

        double totalmigration = 0;
        foreach (var province in Provinces)
        {
            double amountleavingmuit = 1;
            if (province.RankByDevelopment <= 15)
                amountleavingmuit = 1 - (Math.Pow(17 - province.RankByDevelopment, 0.15) - 1);
            var migration = province.Population * Defines.NProvince[NProvince.BASE_MIGRATION_RATE];
            totalmigration += migration*amountleavingmuit;
        }

        totalmigration = totalmigration / 30 / 24;

        double migrantsperattraction = totalmigration / totalattractionpoints;

        long totalchange = 0;
        foreach(var province in Provinces)
        {
            double amountleavingmuit = 1;
            if (province.RankByDevelopment <= 15)
                amountleavingmuit = 1 - (Math.Pow(17 - province.RankByDevelopment, 0.15) - 1);

            double leaving = -(province.Population * Defines.NProvince[NProvince.BASE_MIGRATION_RATE] / 30 / 24);
            leaving *= amountleavingmuit;

            double netchange = leaving;
            netchange += Math.Pow(province.MigrationAttraction, 1.025) * migrantsperattraction;

            province.Population += (int)netchange;

            province.MonthlyEstimatedMigrants = (int)(netchange * 30 * 24);
            totalchange += province.MonthlyEstimatedMigrants;
        }
    }

    public void UpdateOrAddModifier(DistrictModifierType type, double value) {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = new() { Amount = value, ModifierType = type };
        else
            Modifiers[type].Amount += value;
    }

    public void UpdateModifiers() {
        Modifiers = new();
        var value_executionstate = new ExecutionState(this, null, parentscopetype:ScriptScopeType.District);
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var staticmodifier in StaticModifiers) {
            foreach (var modifiernode in staticmodifier.BaseStaticModifiersObj.ModifierNodes) {
                var value = (double)modifiernode.GetValue(value_executionstate, staticmodifier.ScaleBy);
                UpdateOrAddModifier((DistrictModifierType)modifiernode.districtModifierType!, value);
            }
        }
    }
}