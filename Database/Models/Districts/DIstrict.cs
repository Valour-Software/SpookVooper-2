using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Managers;

namespace SV2.Database.Models.Districts;

public class DistrictModifier
{
    public DistrictModifierType ModifierType { get; set; }
    public double Amount { get; set; }  
}

public class District
{
    [Key]
    public long Id { get; set; }

    [VarChar(64)]
    public string? Name { get; set; }

    [NotMapped]
    public string ScriptName => Name.Replace(" ", "_");

    [VarChar(512)]
    public string? Description { get; set; }

    [NotMapped]
    public List<Province> Provinces { get; set; }

    [NotMapped]
    public List<City> Cities => DBCache.GetAll<City>().Where(x => x.DistrictId == Id).ToList();

    [NotMapped]
    public List<SVUser> Citizens => DBCache.GetAll<SVUser>().Where(x => x.DistrictId == Id).ToList();

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

    public Group Group => DBCache.Get<Group>(GroupId)!;

    public long GroupId { get; set; }

    [NotMapped]
    public Senator Senator => DBCache.Get<Senator>(Id);

    public long? GovernorId { get; set; }

    [VarChar(128)]
    public string? FlagUrl { get; set; }

    [NotMapped]
    public Dictionary<DistrictModifierType, DistrictModifier> Modifiers = new();

    [NotMapped]
    [JsonIgnore]
    public string Color => Name switch
    {
        "Lanatia" => "F4B7FD",
        "New Vooperis" => "FEEAB7",
        "Avalon" => "CAFDB8",
        "Elysian Katonia" => "B8B7FD",
        "Ardenti Terra" => "B7BCFC",
        "Kogi" => "B6EEFD",
        "Landing Cove" => "FDB7B7",
        "New Avalon" => "D3FCB6",
        "New Spudland" => "EAB7FC",
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
}