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
    public BaseEntity Owner { get;}
}

public abstract class BaseEntity
{
    [Key]
    public long Id {get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(512)]
    public string Description { get; set; }

    [DecimalType]
    public decimal Credits { get; set;}

    [DecimalType]
    public decimal TaxAbleBalance { get; set;}

    [DecimalType]
    public decimal TaxAbleBalanceYesterday { get; set; }

    // TODO: change this to use an actual db table
    public List<decimal>? CreditSnapshots { get; set;}
    
    [JsonIgnore]
    [VarChar(36)]
    public string ApiKey { get; set; }
    public string ImageUrl { get; set; }

    public long DistrictId { get; set; }

    public virtual EntityType EntityType { get; }

    public static BaseEntity? Find(long Id) => DBCache.FindEntity(Id);

    public async Task DoIncomeTax()
    {
        if (TaxAbleBalance <= 0.0m)
            return;

        if (CreditSnapshots is null)
            CreditSnapshots = new();

        decimal amount = TaxAbleBalance-TaxAbleBalanceYesterday;
        var toprocess = amount;

        if (amount <= 0.0m)
            return;

        decimal totaldue = 0.0m;

        // do district level taxes
        List<TaxPolicy> policies = EntityType switch
        {
            EntityType.Group => DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId && x.taxType == TaxType.GroupIncome).OrderBy(x => x.Minimum).ToList(),
            EntityType.Corporation => DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId && x.taxType == TaxType.CorporateIncome).OrderBy(x => x.Minimum).ToList(),
            EntityType.User => DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId && x.taxType == TaxType.PersonalIncome).OrderBy(x => x.Minimum).ToList()
        };

        foreach(TaxPolicy policy in policies)
        {
            var _amount = policy.GetTaxAmount(amount);
            totaldue += _amount;
            policy.Collected += _amount;
            toprocess -= policy.Maximum;
            if (amount <= 0.0m) 
                break;
        }
        if (totaldue > 0.1m) {
            Transaction taxtrans = new Transaction(Id, DistrictId, totaldue, TransactionType.TaxPayment, $"Income Tax Payment for ¢{TaxAbleBalance - TaxAbleBalanceYesterday} of income.");
            taxtrans.NonAsyncExecute(true);
        }

        amount = TaxAbleBalance-TaxAbleBalanceYesterday;
        toprocess = amount;
        totaldue = 0.0m;

        // now do imperial level taxes
        policies = EntityType switch
        {
            EntityType.Group => DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 && x.taxType == TaxType.GroupIncome).OrderBy(x => x.Minimum).ToList(),
            EntityType.Corporation => DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 && x.taxType == TaxType.CorporateIncome).OrderBy(x => x.Minimum).ToList(),
            EntityType.User => DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == 100 && x.taxType == TaxType.PersonalIncome).OrderBy(x => x.Minimum).ToList()
        };

        foreach(TaxPolicy policy in policies)
        {
            var _amount = policy.GetTaxAmount(amount);
            totaldue += _amount;
            policy.Collected += _amount;
            toprocess -= policy.Maximum;
            if (amount <= 0.0m)
                break;
        }
        if (totaldue > 0.1m) {
            Transaction taxtrans = new Transaction(Id, DistrictId!, totaldue, TransactionType.TaxPayment, $"Income Tax Payment for ¢{TaxAbleBalance - TaxAbleBalanceYesterday} of income.");
            taxtrans.NonAsyncExecute(true);
        }

        // do district level balance tx
        TaxPolicy? _policy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == DistrictId && x.taxType == TaxType.UserBalance);
        if (_policy is not null) {
            totaldue = _policy.GetTaxAmount(Credits);
            if (totaldue > 0.1m) {
                Transaction taxtrans = new(Id, DistrictId!, totaldue, TransactionType.TaxPayment, $"Balance Tax Payment tax id: {_policy.Id}");
                taxtrans.NonAsyncExecute(true);
                _policy.Collected += totaldue;
            }
        }

    }

    public bool HasPermission(BaseEntity entity, GroupPermission permission)
    {
        return false;
    }

    public static BaseEntity? FindByApiKey(string apikey)
    {
        BaseEntity? entity = DBCache.GetAll<Group>().FirstOrDefault(x => x.ApiKey == apikey);
        if (entity is null) {
            entity = DBCache.GetAll<User>().FirstOrDefault(x => x.ApiKey == apikey);
        }
        return entity;
    }
}