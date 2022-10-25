using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Economy;
using System.Threading.Tasks;

namespace SV2.Database.Models.Entities;

public enum EntityType
{
    User,
    Group,
    Corporation,
    CreditAccount
}

public interface IHasOwner
{
    public long OwnerId { get; set; }
    public IEntity Owner { get;}
}

public interface IEntity
{
    // the id will be in the following format:
    // x-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    // ex: u-c60c6bd8-0409-4cbd-8bb8-3c87e24c55f8
    [Key]
    public long Id {get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(512)]
    public string Description { get; set; }
    public decimal Credits { get; set;}
    public decimal TaxAbleCredits { get; set;}
    public List<decimal>? CreditSnapshots { get; set;}
    
    [JsonIgnore]
    [VarChar(36)]
    public string Api_Key { get; set; }
    public string Image_Url { get; set; }

    public long DistrictId { get; set; }

    public EntityType entityType { get; }

    public static IEntity? Find(long Id)
    {
        return DBCache.FindEntity(Id);
    }

    public async Task DoIncomeTax()
    {

        if (TaxAbleCredits <= 0.0m) {
            return;
        }

        if (CreditSnapshots is null) {
            CreditSnapshots = new();
        }

        decimal amount = TaxAbleCredits-CreditSnapshots.TakeLast(7).Sum();
        if (amount <= 0.0m) {
            return;
        }
        decimal totaldue = 0.0m;

        // do district level taxes
        List<TaxPolicy> policies = new();
        switch (entityType)
        {
            case EntityType.Group:
                policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId && x.taxType == TaxType.GroupIncome).OrderBy(x => x.Minimum).ToList();
                break;
            case EntityType.Corporation:
                policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId && x.taxType == TaxType.CorporateIncome).OrderBy(x => x.Minimum).ToList();
                break;
            case EntityType.User:
                policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId && x.taxType == TaxType.PersonalIncome).OrderBy(x => x.Minimum).ToList();
                break;
        }
        foreach(TaxPolicy policy in policies)
        {
            totaldue += policy.GetTaxAmount(amount);
            policy.Collected += policy.GetTaxAmount(amount);
            amount -= policy.Maximum;
            if (amount <= 0.0m) {
                break;
            }
        }

        if (totaldue > 0.01m) {
            Transaction taxtrans = new Transaction(Id, DistrictId, totaldue, TransactionType.TaxPayment, $"Income Tax Payment");
            taxtrans.NonAsyncExecute(true);
        }

        amount = TaxAbleCredits-(CreditSnapshots.TakeLast(7).Sum()/7);
        totaldue = 0.0m;

        // now do imperial level taxes
        switch (entityType)
        {
            case EntityType.Group:
                policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 && x.taxType == TaxType.GroupIncome).OrderBy(x => x.Minimum).ToList();
                break;
            case EntityType.Corporation:
                policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 && x.taxType == TaxType.CorporateIncome).OrderBy(x => x.Minimum).ToList();
                break;
            case EntityType.User:
                policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 && x.taxType == TaxType.PersonalIncome).OrderBy(x => x.Minimum).ToList();
                break;
        }

        // do imperial level taxes
        foreach(TaxPolicy policy in policies)
        {
            totaldue += policy.GetTaxAmount(amount);
            amount -= policy.Maximum;
            policy.Collected += policy.GetTaxAmount(amount);
            if (amount <= 0.0m) {
                break;
            }
        }
        if (totaldue > 0.01m) {
            Transaction taxtrans = new Transaction(Id, DistrictId!, totaldue, TransactionType.TaxPayment, $"Income Tax Payment for ¢{amount} income");
            taxtrans.NonAsyncExecute(true);
        }

        // do district level balance tx
        TaxPolicy? _policy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == DistrictId && x.taxType == TaxType.UserBalance);
        if (_policy is not null) {
            totaldue = _policy.GetTaxAmount(Credits);
            if (totaldue > 0.01m) {
                Transaction taxtrans = new Transaction(Id, DistrictId!, totaldue, TransactionType.TaxPayment, $"Balance Tax Payment tax id: {_policy.Id}");
                taxtrans.NonAsyncExecute(true);
                _policy.Collected += totaldue;
            }
        }

    }

    public bool HasPermission(IEntity entity, GroupPermission permission);
    public static IEntity? FindByApiKey(string apikey)
    {
        IEntity? entity = DBCache.GetAll<Group>().FirstOrDefault(x => x.Api_Key == apikey);
        if (entity is null) {
            entity = DBCache.GetAll<User>().FirstOrDefault(x => x.Api_Key == apikey);
        }
        return entity;
    }
}