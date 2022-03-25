using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Users;

namespace SpookVooper_2.Database.Models.Entities;

public enum EntityType
{
    User,
    Group,
    GroupRole,
    CreditAccount,
    Division,
    Regiment,
    TradeItem,
    TradeItemDefinition,
    TaxPolicy,
    TaxCreditPolicy,
    Transaction,
    StockOffer,
    StockObject,
    None
}

public class Entity
{
    // the id will be in the following format:
    // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    // ex: c60c6bd8-0409-4cbd-8bb8-3c87e24c55f8
    // Groups, Users, Credit Accounts may NOT share the same Id
    [Key]
    public string Id { get; set;}
    public string? Name { get; set;}
    public string? description { get; set; }
    
    public EntityType Type { get; set;}
}