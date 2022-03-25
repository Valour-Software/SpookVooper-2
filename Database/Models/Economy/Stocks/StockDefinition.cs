using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy.Stocks;

public class StockDefinition
{
    [Key]
    public string Ticker { get; set;}

    // The group that issued this stock
    public string Group_Id { get; set; }

    // Current value estimate
    public decimal Current_Value { get; set; }
}