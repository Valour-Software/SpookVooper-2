using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.Database.Models.Factories;

public class Mine : IHasOwner
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.FindAsync(OwnerId).GetAwaiter().GetResult();
        }
    }

    public string CountyId { get; set; }

    // the name of the resource that this mine mines
    public string ResourceName { get; set; }

    public int Level { get; set; }
    public bool HasAEmployee { get; set; }

    // amount of ResourceName that this mine produces per hour
    public decimal Rate { get; set;}

    // factories will get damaged from Natural Disasters which occurs from events and from VOAA
    public decimal Damage { get; set; }

    public async Task Tick(List<TradeItem> tradeItems)
    {
        // TODO: when we add district stats (industal stat, etc) update this
        decimal ProductionBonus = 1.0m;
        if (HasAEmployee) {
            ProductionBonus += 0.5m;
        };

        if (Damage < 0.99m) {
            double diff = Math.Abs((double)Damage-1);
            decimal Reduction = (decimal)Math.Pow(diff+1,5)/10m;
            // examples
            // 10% damage = 6% reduction
            // 20% damage = 25% reduction
            // 30% damage = 37% reduction

            ProductionBonus /= Reduction;
        }

        // find the tradeitem
        TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == ResourceName && x.Definition.OwnerId == "g-vooperia" && x.OwnerId == OwnerId);
        if (item is null) {
            item = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = OwnerId,
                Definition_Id = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Name == ResourceName && x.OwnerId == "g-vooperia").Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(item.Id, item);
            await VooperDB.Instance.TradeItems.AddAsync(item);
            await VooperDB.Instance.SaveChangesAsync();
        }
        item.Amount += Rate*ProductionBonus;
    }

}