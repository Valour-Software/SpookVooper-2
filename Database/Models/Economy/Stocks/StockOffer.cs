using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy.Stocks;

public enum OrderType
{
    Buy,
    Sell
}

public class StockOffer : IHasOwner
{
    [Key]
    public string Id { get; set;}

    // Owner of this offer
    public string Owner_Id { get; set; }
    
    // The ticker of the stock in this offer
    public string Ticker { get; set;}

    public OrderType orderType { get; set;}

    // The target price for this order, also known as a "ASK" or "BID" value
    public decimal Target { get; set;}

    // The amount of stock in this offer
    public int Amount { get; set;}
}