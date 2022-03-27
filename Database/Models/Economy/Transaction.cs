using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy;

public enum TransactionType
{
    Loan,
    // also includes trading resources
    ItemTrade,
    Paycheck,
    StockTrade,
    // use this when the transaction does not fit the other types
    Payment
}

public class Transaction
{
    [Key]
    public string Id { get; set; }
    public decimal Credits { get; set; }
    public DateTime Time { get; set; }
    public string FromId { get; set; }
    public string ToId { get; set; }
    public TaxType taxType { get; set;}
    public TransactionType transactionType { get; set;}
}