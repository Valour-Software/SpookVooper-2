using SV2.Database.Models.Districts;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Valour.Api.Models;

namespace SV2.Database.Models.TradeDeals;

public class TradeDealBasePart
{
    public TradeDealPartType Type { get; set; }
    public long Amount { get; set; }
    public Frequency Frequency { get; set; }
    public long TargetEntityId { get; set; }
    public long OfferorEntityId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public BaseEntity TargetEntity => BaseEntity.Find(TargetEntityId);

    [NotMapped]
    [JsonIgnore]
    public BaseEntity OfferorEntity => BaseEntity.Find(OfferorEntityId);

    /// <summary>
    /// This is called either once, or every period (as defined by Frequency)
    /// </summary>
    /// <returns></returns>
    public async Task Execute(TradeDeal tradeDeal)
    {
        if (Type == TradeDealPartType.Credits)
        {
            var tran = new SVTransaction(OfferorEntity, TargetEntity, Amount, TransactionType.Payment, $"Trade Deal with id {tradeDeal.Id}");

            // TODO: check to see if valour just went down
            // aka see if the OfferorEntity actually lacks the credits or another error has just occured.
            // also you can NOT execute any part of the tradedeal until you have made sure that every trade part can be executed sucucefully.
            var result = await tran.Execute();
        }
    }
}
