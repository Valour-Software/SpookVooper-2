using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using System.Threading.Tasks;

namespace SV2.Database.Models.Entities;

public enum EntityType
{
    User,
    Group,
    CreditAccount
}

public interface IHasOwner
{
    public string OwnerId { get; set; }
    public IEntity Owner { get;}
}

public interface IEntity
{
    // the id will be in the following format:
    // x-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    // ex: u-c60c6bd8-0409-4cbd-8bb8-3c87e24c55f8
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    decimal Credits { get; set;}
    decimal CreditsYesterday { get; set;}
    public string Api_Key { get; set; }
    public string Image_Url { get; set; }
    public string? DistrictId { get; set; }
    public static IEntity? Find(string Id)
    {
        return DBCache.FindEntity(Id);
    }
}