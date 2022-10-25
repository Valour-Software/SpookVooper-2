using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy.Stocks;

public enum OrderType
{
    Buy,
    Sell
}

public class StockOffer : IHasOwner
{
    [Key]
    public long Id {get; set; }

    // Owner of this offer
    public long OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { get; set; }
    
    // The ticker of the stock in this offer
    [VarChar(4)]
    public string Ticker { get; set;}

    public OrderType orderType { get; set;}

    // The target price for this order, also known as a "ASK" or "BID" value
    public decimal Target { get; set;}

    // The amount of stock in this offer
    public int Amount { get; set;}
}