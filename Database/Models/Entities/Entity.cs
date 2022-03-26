using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Users;
using SpookVooper_2.Database.Models.Groups;
using SpookVooper_2.Database.Models.Economy;

namespace SpookVooper_2.Database.Models.Entities;

public enum EntityType
{
    User,
    Group,
    CreditAccount
}

public interface IHasOwner
{
    public string Owner_Id { get; set; }

    public async Task<IEntity> GetOwner()
    {
        return await IEntity.FindAsync(Owner_Id);
    }
}

public interface IEntity
{
    // the id will be in the following format:
    // x-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    // ex: u-c60c6bd8-0409-4cbd-8bb8-3c87e24c55f8
    [Key]
    public string Id { get; set; }
    public string Name { get; }
    public string Description { get; set; }
    decimal Credits { get; set;}
    decimal CreditsYesterday { get; set;}
    public string Api_Key { get; set; }
    public string Image_Url { get; set; }
    public static async Task<IEntity> FindAsync(string Id)
    {
        // TODO: Update this function to find the object from the database
        switch (Id.Substring(0, 1))
        {
            case "g":
                return (IEntity)new Group();
            case "u":
                return (IEntity)new User();
            case "a":
                return (IEntity)new CreditAccount();
            default:
                return null;
        }
    }
}