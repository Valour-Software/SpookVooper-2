using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.Database.Models.Military;

public enum RegimentType
{
    Infantry = 1,
    Artillery = 2,
    Tank = 3,
    Mech = 4,
}

// Represents the current equipment of the division
public class DivisionEquipment
{
    [Key]
    public long Id {get; set; }
    public string ItemName { get; set;}
    
    public long tradeItemId { get; set; }
    
    [NotMapped]
    // the item that is currently selected to be used
    public SVItemOwnership tradeItem {
        get {
            return DBCache.Get<SVItemOwnership>(tradeItemId)!;
        }
    }

    public long DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; }
}

// for example, an Infantry Regiment might have 5k or 500k troops in it
public class Regiment
{
    [Key]
    public long Id {get; set; }
    public RegimentType Type { get; set;}

    // number of things in this regiment
    // for example in an Infantry Regiment, Count will be the number of soldiers
    // only allowed values are in 100 increments for infantry and 1 increments for everything else
    public int Count { get; set;}

    public long DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; }

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
        return new List<KeyValuePair<string, double>> {};
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

public class Division : IHasOwner
{
    [Key]
    public long Id {get; set; }

    [InverseProperty("Division")]
    public ICollection<DivisionEquipment> Equipment { get; set; }

    [InverseProperty("Division")]
    public ICollection<Regiment> Regiments { get; set; }
    // current manpower in this division
    public int ManPower { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    public long OwnerId { get; set; }

    [NotMapped]
    public BaseEntity Owner { 
        get {
            return BaseEntity.Find(OwnerId)!;
        }
    }

    // current effectiveness of this division
    public decimal CombatEffectiveness { get; set; }

    public decimal Xp { get; set; }

    [NotMapped]
    public int Level {
        get {
            return GetLevel(Xp);
        }
    }
    
    public int GetLevel(decimal xp)
    {
        if (xp > 10_000) {
            return 4;
        }
        if (xp > 5_000) {
            return 3;
        }
        if ( xp > 2_000) {
            return 2;
        }
        return 1;
    }

    // the id of the Province that this unit is currently in
    public int Province { get; set; }

    public decimal GetAttack()
    {
        decimal attack = 0;
        foreach(Regiment regiment in Regiments) 
        {
            string MainEquipmentNeeded = regiment.GetWeapon();
            SVItemOwnership EquipmentItem = Equipment.FirstOrDefault(x => x.ItemName == MainEquipmentNeeded).tradeItem;
            //attack += EquipmentItem.Definition.BuiltinModifiers.FirstOrDefault(x => x.ModifierType == BuildInModifierTypes.Attack)!.ModifierLevelDefinition.ModifierValue*regiment.Count;
        }
        attack *= CombatEffectiveness;
        return attack;
    }

    public double GetCombatEffectiveness()
    {
        // CombatEffectiveness is computed as which of the following has the lowest ratio:
        // 1. ManPower / ManPowerNeeded
        // 2. Equipment in storage / EquipmentNeeded
        
        int totalManPowerNeeded = Regiments.Sum(x => x.Count);
        double manPowerEffectiveness = ManPower/totalManPowerNeeded;
        double totalEquipmentNeed = 0;
        double currentequipment = 0;
        foreach (Regiment regiment in Regiments) {
            foreach(KeyValuePair<string, double> equipmentNeed in regiment.GetEquipmentNeeds()) {
                totalEquipmentNeed += equipmentNeed.Value;
                currentequipment += Math.Min(equipmentNeed.Value, Equipment.First(x => x.ItemName == equipmentNeed.Key).tradeItem.Amount);
            }
        }
        double equipmentEffectiveness = currentequipment/totalEquipmentNeed;
        return Math.Min(manPowerEffectiveness, equipmentEffectiveness);
    }
}