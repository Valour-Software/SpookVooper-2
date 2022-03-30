using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;
using SV2.Web;

namespace SV2.Database.Models.Economy;

public enum TransactionType
{
    Loan = 1,
    // also includes trading resources
    ItemTrade = 2,
    Paycheck = 3,
    StockTrade = 4,
    // use this when the transaction does not fit the other types
    Payment = 5,
    // only issued by governmental bodies
    TaxCreditPayment = 6,
    TaxPayment = 7
}

public class Transaction
{
    [Key]
    public string Id { get; set; }
    public decimal Credits { get; set; }
    public DateTime Time { get; set; }
    public string FromId { get; set; }
    public string ToId { get; set; }
    public TransactionType transactionType { get; set; }
    public string Details { get; set; }

    public async Task<TaskResult> Execute(bool Force = false)
    {
        if (!Force && Credits < 0)
        {
            return new TaskResult(false, "Transaction must be positive.");
        }
        if (Credits == 0)
        {
            return new TaskResult(false, "Transaction must have a value.");
        }
        if (FromId == ToId)
        {
            return new TaskResult(false, $"An entity cannot send credits to itself.");
        }

        IEntity? fromEntity = await IEntity.FindAsync(FromId);
        IEntity? toEntity = await IEntity.FindAsync(ToId);

        if (fromEntity == null) { return new TaskResult(false, $"Failed to find sender {FromId}."); }
        if (toEntity == null) { return new TaskResult(false, $"Failed to find reciever {ToId}."); }

        if (!Force && fromEntity.Credits < Credits)
        {
            return new TaskResult(false, $"{fromEntity.Name} cannot afford to send ¢{Credits}");
        }

        decimal totaltaxpaid = 0.0m;

        if (transactionType != TransactionType.TaxPayment) {

            List<TaxPolicy> policies = (await VooperDB.GetTaxPoliciesAsync()).Where(x => x.DistrictId == null || x.DistrictId == fromEntity.DistrictId || x.DistrictId == toEntity.DistrictId).ToList();

            // must do TAXES (don't let Etho see this)

            foreach (TaxPolicy policy in policies)
            {
                decimal amount = 0.0m;
                switch (policy.taxType) 
                {
                    case TaxType.Transactional:
                        amount = policy.GetTaxAmount(Credits);
                        break;
                    case TaxType.Sales:
                        if (transactionType == TransactionType.ItemTrade) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.StockBought:
                        if (transactionType == TransactionType.StockTrade) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.StockSale:
                        if (transactionType == TransactionType.StockTrade) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.Payroll:
                        if (transactionType == TransactionType.Paycheck) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                }
                // continue if transaction did not match the tax policy
                if (amount == 0.0m) {
                    continue;
                }
                if (policy.DistrictId is null) {
                    string _FromId = FromId;
                    if (policy.taxType == TaxType.Sales || policy.taxType == TaxType.Transactional || policy.taxType == TaxType.Payroll) {
                        _FromId = ToId;
                    }
                    Transaction taxtrans = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Credits = amount,
                        Time = DateTime.UtcNow,
                        FromId = _FromId,
                        ToId = "g-vooperia",
                        transactionType = TransactionType.TaxPayment,
                        Details = $"Tax payment for transaction id: {Id}. Tax Id: {policy.Id}"
                    };
                    policy.Collected += amount;
                    totaltaxpaid += amount;
                    await taxtrans.Execute(true);
                }
                else {
                    if (policy.DistrictId == fromEntity.DistrictId && policy.taxType != TaxType.Sales && policy.taxType != TaxType.Payroll && policy.taxType != TaxType.Transactional) {
                        Transaction taxtrans = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Credits = amount,
                            Time = DateTime.UtcNow,
                            FromId = FromId,
                            ToId = policy.DistrictId,
                            transactionType = TransactionType.TaxPayment,
                            Details = $"Tax payment for transaction id: {Id}. Tax Id: {policy.Id}"
                        };
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        await taxtrans.Execute(true);
                    }
                    else if (policy.DistrictId == toEntity.DistrictId){
                        Transaction taxtrans = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Credits = amount,
                            Time = DateTime.UtcNow,
                            FromId = toEntity.Id,
                            ToId = policy.DistrictId,
                            transactionType = TransactionType.TaxPayment,
                            Details = $"Tax payment for transaction id: {Id}. Tax Id: {policy.Id}"
                        };
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        await taxtrans.Execute(true);
                    }
                }
            }
        }

        fromEntity.Credits -= Credits;
        toEntity.Credits += Credits;

        return new TaskResult(true, $"Successfully sent ¢{Credits} to {toEntity.Name} with ¢{totaltaxpaid} tax.");

    }
}