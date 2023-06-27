using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Military;

public enum RegimentType
{
    Infantry = 1,
    Artillery = 2,
    Tank = 3,
    Mech = 4,
}

public class Division
{
}

public class Regiment
{
    public long Id { get; set; }
    public RegimentType Type { get; set; }

    // number of things in this regiment
    // for example in an Infantry Regiment, Count will be the number of soldiers
    // only allowed values are in 1k increments for infantry and 1 increments for everything else
    public int Count { get; set; }
    public long DivisionId { get; set; }
    public List<KeyValuePair<string, double>> GetEquipmentNeeds()
    {
        // NOTE: 1 of Infantry equipment is enough for 100 troops that uses that equipment, anything else is 1 for 1
        // for example 100k Infantry needs 1000 Guns & 1000 Ammo.
        switch (Type)
        {
            case RegimentType.Infantry:
                return new List<KeyValuePair<string, double>> {
                    KeyValuePair.Create("Ammo", Count/100.0),
                    KeyValuePair.Create("Rifle", Count/100.0)
                };
        }
        return new List<KeyValuePair<string, double>> { };
    }

    public static List<string> GetBaseEquipmentNeeded(RegimentType type)
    {
        return type switch
        {
            RegimentType.Infantry => new() { "Rifle", "Ammo"}
        };
    }

    public string GetWeapon()
    {
        switch (Type)
        {
            case RegimentType.Infantry:
                return "Rifle";
        }
        return "";
    }
}