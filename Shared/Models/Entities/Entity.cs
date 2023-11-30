using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Shared.Models.Items;
using Shared.Models.Districts;

namespace Shared.Models.Entities;

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
    public BaseEntity Owner { get; }
}

[JsonDerivedType(typeof(Group), 0)]
[JsonDerivedType(typeof(SVUser), 1)]
public abstract class BaseEntity : Item
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }

    public string? Description { get; set; }
    public decimal Credits { get; set; }
    public decimal TaxAbleBalance { get; set; }
    public string ApiKey { get; set; }

    public string ImageUrl { get; set; }

    public long? DistrictId { get; set; }

    //public District District => DBCache.Get<District>(DistrictId)!;

    public Dictionary<long, SVItemOwnership> SVItemsOwnerships { get; set; }

    public virtual EntityType EntityType { get; set; }

    /// <summary>
    /// Returns the item for the given id
    /// </summary>
    public static async ValueTask<BaseEntity> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<BaseEntity>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<BaseEntity>($"api/entities/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}

public class Entity : BaseEntity
{

}

public enum EntityModifierType
{
    FactoryThroughputFactor,
    FactoryEfficiencyFactor,
    FactoryQuantityCapFactor
}