using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.TradeDeals;

/// <summary>
/// A trade deal is a final deal that both sides has accepted
/// </summary>
public class TradeDeal
{
    [Key]
    public long Id { get; set; }

    public long FirstUser { get; set; }
    public long SecondUser { get; set; }
    public int CurrentRound { get; set; }
}
