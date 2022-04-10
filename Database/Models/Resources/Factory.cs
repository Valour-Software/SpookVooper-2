using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.Database.Models.Factories;

public class Recipe
{
    [Key]
    [GuidID]
    public string Id { get; set; }
    public List<string> Inputs_Names { get; set; }
    public List<int> Inputs_Amounts { get; set; }
    public List<string> Output_Names { get; set; }
    public List<int> Output_Amounts { get; set; }
}

public class Factory : IHasOwner
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(1024)]
    public string Description { get; set; }

    [EntityId]
    public string OwnerId { get; set; }

    // %
    public int Efficiency { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    [GuidID]
    public string CountyId { get; set; }

    [GuidID]
    public string RecipeId { get; set; }

    [ForeignKey("RecipeId")]
    public Recipe recipe { get; set; }
    public int Level { get; set; }
    public bool HasAnEmployee { get; set; }

    // factories will get damaged from Natural Disasters which occurs from events and from VOAA
    public double Damage { get; set; }

    /// <summary>
    /// This function is called every hour IRP time, or normally, 3 times per real life hour.
    /// </summary>

    public async Task Tick(List<TradeItem> tradeItems)
    {

        // update efficiency
        // at 30% efficiency, output is 3.7% which results in 0.74% per IRP day or 2.1% per IRP day
        int growth = ((int)(Math.Pow(Efficiency, 0.03))*(100/Efficiency)) / 5;
        growth = (int)Math.Max(growth, 1.5);
        Efficiency += growth / 24;

        // TODO: when we add district stats (industal stat, etc) update this
        double ProductionBonus = 1.0;
        if (HasAnEmployee) {
            ProductionBonus += 0.5;
        };

        if (Damage < 0.99) {
            double diff = Math.Abs(Damage-1);
            double Reduction = Math.Pow(diff+1,5)/10;
            // examples
            // 10% damage = 6% reduction
            // 20% damage = 25% reduction
            // 30% damage = 37% reduction

            ProductionBonus /= Reduction;
        }


        for (int i = 0; i < recipe.Inputs_Amounts.Count; i++)
        {
            // find the tradeitem
            TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == recipe.Inputs_Names[i] && x.Definition.OwnerId == "g-vooperia" && x.OwnerId == OwnerId);
            if (item is null) {
                break;
            }
            int amountNeeded = (int)((double)recipe.Inputs_Amounts[i]*ProductionBonus*((double)Efficiency / 100));
            if (item.Amount < amountNeeded) {
                break;
            }
            item.Amount -= amountNeeded;
        }

        for (int i = 0; i < recipe.Output_Amounts.Count; i++)
        {
            // find the tradeitem
            TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == recipe.Output_Names[i] && x.Definition.OwnerId == "g-vooperia" && x.OwnerId == OwnerId);
            if (item is null) {
                item = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerId = OwnerId,
                    Definition_Id = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Name == recipe.Output_Names[i] && x.OwnerId == "g-vooperia")!.Id,
                    Amount = 0
                };
                await DBCache.Put<TradeItem>(item.Id, item);
                await VooperDB.Instance.TradeItems.AddAsync(item);
                await VooperDB.Instance.SaveChangesAsync();
            }
            item.Amount += (int)((double)recipe.Output_Amounts[i]*ProductionBonus*((double)Efficiency / 100));
        }
    }

}