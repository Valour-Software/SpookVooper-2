using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;
using SV2.Workers;
using SV2.Web;

namespace SV2.Database.Models.Economy;

public enum TransactionType
{
    Loan = 1,
    // also includes trading resources
    ItemTrade = 2,
    Paycheck = 3,

    [Obsolete]
    StockTrade = 4,
    // use this when the transaction does not fit the other types
    Payment = 5,
    // only issued by governmental bodies
    TaxCreditPayment = 6,
    TaxPayment = 7,
    FreeMoney = 8,
    LoanRepayment = 9,
    DividendPayment = 10,
    StockSale = 11,
    StockBrought = 12,
    ResourceSale = 13,
    ResourceBrought = 14,
    UBI = 15
}

public class SVTransaction
{
    [Key]
    public long Id {get; set; }
    public decimal Credits { get; set; }

    public DateTime Time { get; set; }

    public long FromId { get; set; }

    public long ToId { get; set; }
    public TransactionType transactionType { get; set; }

    [VarChar(1024)]
    public string Details { get; set; }
    public bool? IsAnExpense {get; set; }

    [NotMapped]

    public bool IsCompleted = false;

    [NotMapped]

    public TaskResult? Result = null;

    [NotMapped]

    public bool Force = false;

    [NotMapped]
    public BaseEntity FromEntity { get; set; }

    [NotMapped]
    public BaseEntity ToEntity { get; set; }

    public SVTransaction()
    {
        
    }

    public SVTransaction(BaseEntity fromEntity, BaseEntity toEntity, decimal credits, TransactionType TransactionType, string details)
    {
        Id = IdManagers.GeneralIdGenerator.Generate();
        Credits = credits;
        FromId = fromEntity.Id;
        ToId = toEntity.Id;
        Time = DateTime.UtcNow;
        transactionType = TransactionType;
        Details = details;
        FromEntity = fromEntity;
        ToEntity = toEntity;
    }

    public async Task<TaskResult> Execute(bool force = false)
    {
        Force = force;
        TransactionManager.transactionQueue.Enqueue(this);

        while (!IsCompleted) await Task.Delay(1);

        return Result!;
    }

    public void NonAsyncExecute(bool force = false)
    {
        Force = force;
        TransactionManager.transactionQueue.Enqueue(this);
    }

    public async Task<TaskResult> ExecuteFromManager(VooperDB dbctx, bool Force = false)
    {
        // general checking stuff
        if (Force)
            throw new NotImplementedException("Force is fake news!!!!!");

        if (!Force && Credits < 0)
            return new TaskResult(false, "Transaction must be positive.");
        if (Credits == 0)
            return new TaskResult(false, "Transaction must have a value.");
        if (FromId == ToId)
            return new TaskResult(false, $"An entity cannot send credits to itself.");
        if (FromEntity == null || FromEntity.EcoAccountId == 0) { return new TaskResult(false, $"Failed to find sender {FromId}."); }
        if (ToEntity == null || ToEntity.EcoAccountId == 0) { return new TaskResult(false, $"Failed to find reciever {ToId}."); }

        if (!Force && (await FromEntity.GetCreditsAsync()) < Credits)
            return new TaskResult(false, $"{FromEntity.Name} cannot afford to send ¢{Credits}");

        var FromUserId = ValourNetClient.BotId;
        string oauthkey = null;
        if (FromEntity.EntityType == EntityType.User) {
            oauthkey = ((SVUser)FromEntity).OAuthToken;
            FromUserId = ((SVUser)FromEntity).ValourId;
        }

        var ToUserId = ValourNetClient.BotId;
        if (ToEntity.EntityType == EntityType.User)
            ToUserId = ((SVUser)ToEntity).ValourId;

        var transaction = new Transaction()
        {
            PlanetId = VoopAI.VoopAI.PlanetId,
            UserFromId = FromUserId,
            UserToId = ToUserId,
            AccountFromId = FromEntity.EcoAccountId,
            AccountToId = ToEntity.EcoAccountId,
            TimeStamp = DateTime.UtcNow,
            Description = Details,
            Data = "",
            Amount = Credits,
            ForcedBy = ValourNetClient.BotId,
            Fingerprint = Guid.NewGuid().ToString()
        };

        var _result = await ValourNetClient.SendTransactionRequestAsync(transaction, oauthkey);

        decimal totaltaxpaid = 0.0m;

        if (transactionType != TransactionType.TaxPayment && transactionType != TransactionType.Loan)
        {

            List<TaxPolicy> policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == null || x.DistrictId == FromEntity.DistrictId || x.DistrictId == ToEntity.DistrictId).ToList();

            // must do TAXES (don't let Etho see this)

            foreach (TaxPolicy policy in policies)
            {
                if (policy.Rate <= 0.01m)
                {
                    continue;
                }
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
                    case TaxType.StockSale:
                        if (transactionType == TransactionType.StockSale) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.StockBought:
                        if (transactionType == TransactionType.StockBrought) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.ResourceSale:
                        if (transactionType == TransactionType.ResourceSale)
                        {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.ResourceBrought:
                        if (transactionType == TransactionType.ResourceBrought)
                        {
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
                if (amount == 0.0m)
                {
                    continue;
                }
                if (policy.DistrictId == 100)
                {
                    long _FromId = FromId;
                    if (policy.taxType == TaxType.Sales || policy.taxType == TaxType.Transactional || policy.taxType == TaxType.Payroll)
                    {
                        _FromId = ToId;
                    }
                    SVTransaction taxtrans = new SVTransaction(FromEntity, BaseEntity.Find(100), amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
                    policy.Collected += amount;
                    totaltaxpaid += amount;
                    taxtrans.NonAsyncExecute(true);
                }
                else
                {
                    if (policy.DistrictId == FromEntity.DistrictId && policy.taxType != TaxType.Sales && policy.taxType != TaxType.Payroll && policy.taxType != TaxType.Transactional)
                    {
                        SVTransaction taxtrans = new SVTransaction(FromEntity, BaseEntity.Find(policy.DistrictId), amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        taxtrans.NonAsyncExecute(true);
                    }
                    else if (policy.DistrictId == ToEntity.DistrictId)
                    {
                        SVTransaction taxtrans = new SVTransaction(ToEntity, BaseEntity.Find(policy.DistrictId), amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        taxtrans.NonAsyncExecute(true);
                    }
                }
            }
        }

        //fromEntity.Credits -= Credits;
        //toEntity.Credits += Credits;

        if (transactionType is TransactionType.Paycheck) 
        {
            FromEntity.TaxAbleBalance -= Credits;
            ToEntity.TaxAbleBalance += Credits;
        }

        if (transactionType is TransactionType.DividendPayment
            or TransactionType.ItemTrade
            or TransactionType.Payment
            or TransactionType.StockSale
            or TransactionType.ResourceSale)
        {
            if (IsAnExpense is not null and true)
                FromEntity.TaxAbleBalance -= Credits;
            ToEntity.TaxAbleBalance += Credits;
        }

        else if (transactionType == TransactionType.ResourceBrought)
        {
            if (IsAnExpense is not null and true)
                FromEntity.TaxAbleBalance -= Credits;
            ToEntity.TaxAbleBalance += Credits;
        }

        else if (transactionType == TransactionType.StockBrought)
        {
            ToEntity.TaxAbleBalance += Credits;
        }

        else if (transactionType == TransactionType.TaxPayment) {
            // we do this so that districts, states, and province groups can have a "profit" for banks and loan brokers to use.
            ToEntity.TaxAbleBalance += Credits;
        }

        dbctx.Transactions.Add(this);


        var worked = _result is not null;
        return new TaskResult(worked, worked ? $"Successfully sent ¢{Credits} to {ToEntity.Name} with ¢{totaltaxpaid} tax." : "Failed");
    }

    public async Task<TaskResult> OldExecuteFromManager(VooperDB dbctx, bool Force = false)
    {

        while (TransactionManager.ActiveSvids.Contains(FromId) || TransactionManager.ActiveSvids.Contains(ToId))
        {
            await Task.Delay(1);
        }

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

        BaseEntity? fromEntity = BaseEntity.Find(FromId);
        BaseEntity? toEntity = BaseEntity.Find(ToId);

        if (fromEntity == null) { return new TaskResult(false, $"Failed to find sender {FromId}."); }
        if (toEntity == null) { return new TaskResult(false, $"Failed to find reciever {ToId}."); }

        TransactionManager.ActiveSvids.Add(FromId);
        TransactionManager.ActiveSvids.Add(ToId);

        if (!Force && (await fromEntity.GetCreditsAsync()) < Credits)
        {
            TransactionManager.ActiveSvids.Remove(FromId);
            TransactionManager.ActiveSvids.Remove(ToId);
            return new TaskResult(false, $"{fromEntity.Name} cannot afford to send ¢{Credits}");
        }

        decimal totaltaxpaid = 0.0m;

        if (transactionType != TransactionType.TaxPayment && transactionType != TransactionType.Loan) {

            List<TaxPolicy> policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 || x.DistrictId == fromEntity.DistrictId || x.DistrictId == toEntity.DistrictId).ToList();

            // must do TAXES (don't let Etho see this)

            foreach (TaxPolicy policy in policies)
            {
                if (policy.Rate <= 0.01m)
                {
                    continue;
                }
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
                    case TaxType.StockSale:
                        if (transactionType == TransactionType.StockSale) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.StockBought:
                        if (transactionType == TransactionType.StockBrought) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.ResourceSale:
                        if (transactionType == TransactionType.ResourceSale)
                        {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.ResourceBrought:
                        if (transactionType == TransactionType.ResourceBrought)
                        {
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
                if (policy.DistrictId == 100) {
                    long _FromId = FromId;
                    if (policy.taxType == TaxType.Sales || policy.taxType == TaxType.Transactional || policy.taxType == TaxType.Payroll) {
                        _FromId = ToId;
                    }
                    SVTransaction taxtrans = new SVTransaction(FromEntity, BaseEntity.Find(100), amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
                    policy.Collected += amount;
                    totaltaxpaid += amount;
                    taxtrans.NonAsyncExecute(true);
                }
                else {
                    if (policy.DistrictId == fromEntity.DistrictId && policy.taxType != TaxType.Sales && policy.taxType != TaxType.Payroll && policy.taxType != TaxType.Transactional) {
                        SVTransaction taxtrans = new SVTransaction(FromEntity, BaseEntity.Find(policy.DistrictId), amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        taxtrans.NonAsyncExecute(true);
                    }
                    else if (policy.DistrictId == ToEntity.DistrictId) {
                        SVTransaction taxtrans = new SVTransaction(ToEntity, BaseEntity.Find(policy.DistrictId), amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        taxtrans.NonAsyncExecute(true);
                    }
                }
            }
        }

        //fromEntity.Credits -= Credits;
        //toEntity.Credits += Credits;

        if (transactionType is TransactionType.Paycheck) 
        {
            fromEntity.TaxAbleBalance -= Credits;
            toEntity.TaxAbleBalance += Credits;
        }

        if (transactionType is TransactionType.DividendPayment
            or TransactionType.ItemTrade
            or TransactionType.Payment
            or TransactionType.StockSale
            or TransactionType.ResourceSale)
        {
            if (IsAnExpense is not null and true)
                fromEntity.TaxAbleBalance -= Credits;
            toEntity.TaxAbleBalance += Credits;
        }

        else if (transactionType == TransactionType.ResourceBrought)
        {
            if (IsAnExpense is not null and true)
                fromEntity.TaxAbleBalance -= Credits;
            toEntity.TaxAbleBalance += Credits;
        }

        else if (transactionType == TransactionType.StockBrought)
        {
            toEntity.TaxAbleBalance += Credits;
        }

        else if (transactionType == TransactionType.TaxPayment) {
            // we do this so that districts, states, and province groups can have a "profit" for banks and loan brokers to use.
            toEntity.TaxAbleBalance += Credits;
        }

        dbctx.Transactions.Add(this);

        TransactionManager.ActiveSvids.Remove(FromId);
        TransactionManager.ActiveSvids.Remove(ToId);

        return new TaskResult(true, $"Successfully sent ¢{Credits} to {toEntity.Name} with ¢{totaltaxpaid} tax.");

    }
}