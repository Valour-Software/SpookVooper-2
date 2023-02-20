using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using Microsoft.EntityFrameworkCore;

namespace SV2.Database.Models.Districts;

public class DistrictModifier
{
    public DistrictModifierType ModifierType { get; set; }
    public double Amount { get; set; }  
}

public class District
{
    [Key]
    public long Id {get; set; }

    [VarChar(64)]
    public string? Name { get; set;}

    [NotMapped]
    public string ScriptName => Name.Replace(" ", "_");

    [VarChar(512)]
    public string? Description { get; set; }

    [InverseProperty("District")]
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

    public long? GovernorId { get; set;}

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
}