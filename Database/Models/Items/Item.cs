using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Items;

public class TradeItem :  IHasOwner
{
    [Key]
    public string Id { get; set; }
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { get; set; }
    public string Definition_Id { get; set; }
    
    [ForeignKey("DefinitionId")]
    public TradeItemDefinition Definition { get; set; }
    public decimal Amount { get; set;}
    public async Task Give(IEntity entity, decimal amount) 
    {
        // check if the entity we are sending already has this TradeItem
        TradeItem item = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.OwnerId == entity.Id && x.Definition_Id == Definition_Id);
        
        // if null then create one

        if (item is null) 
        {
            item = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = entity.Id,
                Definition_Id = Definition_Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(item.Id, item);
            await VooperDB.Instance.TradeItems.AddAsync(item);
            await VooperDB.Instance.SaveChangesAsync();
        }

        item.Amount += amount;
    }
}

public class TradeItemDefinition : IHasOwner
{
    public string Id { get; set; }
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { get; set; }
    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; }
    // json list of modifiers
    public string Modifiers { get; set; }
}