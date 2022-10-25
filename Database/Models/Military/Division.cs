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
    public TradeItem tradeItem {
        get {
            return DBCache.Get<TradeItem>(tradeItemId)!;
        }
    }

    public long DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; }
}

// NOTE: there can only be 1 Regiment of a type per Division
// an Infantry Regiment might have 5k or 500k troops in it
public class Regiment
{
    [Key]
    public long Id {get; set; }
    public RegimentType Type { get; set;}
    
    // number of things in this regiment
    // for example in an Infantry Regiment, Count will be the number of soldiers
    // only allowed values are in 1k increments
    public int Count { get; set;}

    public long DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; }

    public List<KeyValuePair<string, int>> GetEquipmentNeeds()
    {
        // NOTE: 1 of Infantry equipment is enough for 1k troops that uses that equipment, anything else is 1 for 100 troops
        // for example 100k Infantry needs 100 Guns & 100 Ammo.
        switch (Type)
        {
            case RegimentType.Infantry:
                return new List<KeyValuePair<string, int>> {
                    KeyValuePair.Create("Ammo", Count/1000),
                    KeyValuePair.Create("Rifle", Count/1000)
                };
        }
        return new List<KeyValuePair<string, int>> {};
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
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
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
            TradeItem EquipmentItem = Equipment.FirstOrDefault(x => x.ItemName == MainEquipmentNeeded).tradeItem;
            attack += EquipmentItem.Definition.BuiltinModifiers.FirstOrDefault(x => x.ModifierType == BuildInModifierTypes.Attack)!.ModifierLevelDefinition.ModifierValue*regiment.Count;
        }
        attack *= CombatEffectiveness;
        return attack;
    }

    public decimal GetCombatEffectiveness()
    {
        // CombatEffectiveness is computed as which of the following has the lowest ratio:
        // 1. ManPower / ManPowerNeeded
        // 2. Equipment in storage / EquipmentNeeded
        
        int totalManPowerNeeded = Regiments.Sum(x => x.Count);
        decimal manPowerEffectiveness = ManPower/totalManPowerNeeded;
        decimal totalEquipmentNeed = 0;
        decimal currentequipment = 0;
        foreach (Regiment regiment in Regiments) {
            foreach(KeyValuePair<string, int> equipmentNeed in regiment.GetEquipmentNeeds()) {
                totalEquipmentNeed += (decimal)equipmentNeed.Value;
                currentequipment += Math.Min(equipmentNeed.Value, Equipment.First(x => x.ItemName == equipmentNeed.Key).tradeItem.Amount);
            }
        }
        decimal equipmentEffectiveness = currentequipment/totalEquipmentNeed;
        return Math.Min(manPowerEffectiveness, equipmentEffectiveness);
    }
}