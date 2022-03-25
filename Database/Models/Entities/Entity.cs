using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Users;

namespace SpookVooper_2.Database.Models.Entities;

public enum EntityType
{
    User,
    CreditAccount,
    Division,
    Regiment,
    TradeItem,
    TradeItemDefinition,
    Tax,
    TaxCredit,
    Transaction,
    StockOffer,
    StockObject,
    None
}

public class Entity
{
    [Key]
    public string Id { get; set;}
    public string? Name { get; set;}
    public string? description { get; set; }
    
    public EntityType Type { get; set;}

    public static EntityType GetEntityType(string Id)
    {
        switch (Id.Substring(0, 1))
        {
            case "a":
                return EntityType.CreditAccount; 
            case "u":
                return EntityType.User;
            case "d":
                return EntityType.Division;
            case "r":
                return EntityType.Regiment;
            case "i":
                return EntityType.TradeItem;
            case "t":
                return EntityType.Tax;
            case "o":
                return EntityType.StockOffer;
            case "s":
                return EntityType.StockObject;
            case "p":
                // "p" stands for payment
                return EntityType.Transaction;
            case "c":
                return EntityType.TaxCredit;
            case "b":
                // "b" stands for base
                return EntityType.TradeItemDefinition;
            default:
                return EntityType.None;
        }
    }
}