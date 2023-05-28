using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Shared.Models.Items;

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

    public static async ValueTask<BaseEntity> FindAsync(long id)
    {
        int _id = (int)id;
        Group group = await Group.FindAsync(_id);
        if (group is not null)
            return group;

        SVUser user = await SVUser.FindAsync(_id);
        if (user is not null)
            return user;

        return null;
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