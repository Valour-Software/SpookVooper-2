using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using SV2.Managers;
using SV2.Database.Managers;

namespace SV2.Database.Models.Factories;

public class Factory : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Factory; set => BuildingType = value; }

    /// <summary>
    /// This function is called every IRL hour
    /// </summary>
    public async Task Tick(List<TradeItem> tradeItems)
    {

        if (RecipeId is null) {
            return;
        }

        // TODO: when we add district stats (industal stat, etc) update this

        double rate = 1;

        rate *= Size;

        rate *= Recipe.PerHour;

        rate *= Defines.NProduction["BASE_FACTORY_THROUGHPUT"];

        // ((A2^1.2/1.6)-1)/1000

        if (Quantity <= 0.01)
            Quantity = 0.01;

        if (Quantity < QuantityCap)
        {
            double quantitychange = Defines.NProduction["BASE_QUANTITY_GROWTH_RATE"] / 24;
            quantitychange *= (QuantityCap * QuantityCap) / Quantity;
            Quantity += quantitychange * QuantityGrowthRateFactor;
        }

        rate *= Quantity;

        rate *= ThroughputFactor;

        if (EmployeeId is not null)
            rate *= 2.5;

        /*
        TradeItem? item = null;

        string output = recipe.Output.Key;
        // find the tradeitem
        item = tradeItems.FirstOrDefault(x => x.Definition.Name == output && x.Definition.OwnerId == 100 && x.OwnerId == OwnerId);
        if (item is null) {
            item = new()
            {
                Id = IdManagers.GeneralIdGenerator.Generate(),
                OwnerId = OwnerId,
                Definition_Id = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Name == output && x.OwnerId == 100)!.Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(item.Id, item);
            await VooperDB.Instance.TradeItems.AddAsync(item);
            await VooperDB.Instance.SaveChangesAsync();
        }
        rate *= recipe.Output.Value;
        int wholerate = (int)Math.Floor(rate);
        LeftOver += rate-wholerate;
        if (LeftOver >= 1.0) {
            wholerate += 1;
            LeftOver -= 1.0;
        }
        foreach(string Resource in recipe.Inputs.Keys) 
        {
            item = tradeItems.FirstOrDefault(x => x.Definition.Name == Resource && x.Definition.OwnerId == 100 && x.OwnerId == OwnerId);
            if (item is null) {
                return;
            }
            int amountNeeded = (int)(recipe.Inputs[Resource]*wholerate/Efficiency);
            if (item.Amount < amountNeeded) {
                return;
            }
        }
        foreach(string Resource in recipe.Inputs.Keys) 
        {
            // find the tradeitem
            item = tradeItems.FirstOrDefault(x => x.Definition.Name == Resource && x.Definition.OwnerId == 100 && x.OwnerId == OwnerId);
            int amountNeeded = (int)(recipe.Inputs[Resource]*wholerate/Efficiency);
            item.Amount -= amountNeeded;
        }
        item.Amount += wholerate;
        */
    }
}