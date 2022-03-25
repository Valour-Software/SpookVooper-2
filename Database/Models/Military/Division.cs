using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Military;

public enum RegimentType
{
    Infantry,
    Artillery
}

public enum DivisionEquipmentType
{
    Gun,
    // 1k units will use 1 ammo per hour of fighting
    Ammo
}

// Represents the current equipment of the division
public class DivisionEquipment
{
    public string Id { get; set;}
    public DivisionEquipmentType Type { get; set;}
    public TradeItem tradeItem { get; set;}
}

// NOTE: there can only be 1 Regiment of a type per Division
// an Infantry Regiment might have 5k or 500k troops in it
public class Regiment
{
    public string Id { get; set;}
    public RegimentType Type { get; set;}
    // number of things in this regiment
    // for example in an Infantry Regiment, Count will be the number of soldiers
    // only allowed values are in 1k increments
    public int Count { get; set;}

    public List<List<int>> GetEquipmentNeeds()
    {
        // NOTE: 1 of any equipment is enough for 1k troops that uses that equipment
        // for example 100k Infantry needs 100 Guns & 100 Ammo.
        switch (Type)
        {
            case RegimentType.Infantry:
                return new List<List<int>> {
                    new List<int> {
                        ((int)DivisionEquipmentType.Ammo),
                        (int)Count / 1000
                    },
                    new List<int> {
                        ((int)DivisionEquipmentType.Gun),
                        (int)Count / 1000
                    }
                };
        }
        return new List<List<int>> {};
    }
}

public class Division
{
    public List<DivisionEquipment> Equipment { get; set;}
    public List<Regiment> Regiments { get; set;}
    // current manpower in this division
    public int ManPower { get; set;}
    public string Name { get; set;}
    public string Owner_Id { get; set;}
    [NotMapped]
    public Entity Owner { get; set;}
    // current strength of the division
    public decimal Strength { get; set;}
    public decimal X { get; set;}
    public decimal Y { get; set;}

    public decimal GetStrength()
    {
        // Strength is computed as which of the following has the lowest ratio:
        // 1. ManPower / ManPowerNeeded
        // 2. Equipment in storage / EquipmentNeeded
        
        int totalManPowerNeeded = Regiments.Sum(x => x.Count);
        decimal manPowerStrength = ManPower/totalManPowerNeeded;
        decimal totalEquipmentNeed = 0;
        decimal currentequipment = 0;
        foreach (Regiment regiment in Regiments) {
            foreach(List<int> equipmentNeed in regiment.GetEquipmentNeeds()) {
                totalEquipmentNeed += (decimal)equipmentNeed[1];
                currentequipment += Math.Min(equipmentNeed[1], Equipment.First(x => (int)x.Type == equipmentNeed[0]).tradeItem.Amount);
            }
        }
        decimal equipmentStrength = currentequipment/totalEquipmentNeed;
        return Math.Min(manPowerStrength, equipmentStrength);
    }
}