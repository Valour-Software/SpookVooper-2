using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy.Stocks;

public class StockDefinition
{
    [Key]
    [GuidID]
    public string Ticker { get; set;}

    // The group that issued this stock
    [EntityId]
    public string GroupId { get; set; }

    // Current value estimate
    public decimal Current_Value { get; set; }
}