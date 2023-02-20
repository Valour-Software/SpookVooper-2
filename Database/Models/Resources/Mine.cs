using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using SV2.Database.Managers;

namespace SV2.Database.Models.Factories;

public class Mine : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Factory; set => BuildingType = value; }

    public async Task Tick(List<TradeItem> tradeItems)
    {
        // TODO: when we add district stats (industal stat, etc) update this
        

        double rate = 1;

        if (EmployeeId is not null) {
            // 2.5x production boost if this factory has an employee
            rate *= 2.5;
        };

        rate *= Size;

        rate *= Recipe.PerHour;

        rate *= Defines.NProduction["BASE_MINE_THROUGHPUT"];

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

        /*

        // find the tradeitem
        TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == ResourceName && x.Definition.OwnerId == 100 && x.OwnerId == OwnerId);
        if (item is null) {
            item = new()
            {
                Id = IdManagers.GeneralIdGenerator.Generate(),
                OwnerId = OwnerId,
                Definition_Id = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Name == ResourceName && x.OwnerId == 100)!.Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(item.Id, item);
            await VooperDB.Instance.TradeItems.AddAsync(item);
            await VooperDB.Instance.SaveChangesAsync();
        }
        int wholerate = (int)Math.Floor(rate);
        LeftOver += rate-wholerate;
        if (LeftOver >= 1.0) {
            wholerate += 1;
            LeftOver -= 1.0;
        }
        item.Amount += wholerate;

        // do district taxes

        TaxPolicy? policy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == Province.DistrictId && x.taxType == TaxType.ResourceMined && x.Target == ResourceName);
        if (policy is not null) {
            decimal due = policy.GetTaxAmountForResource(wholerate);
            Transaction taxtrans = new Transaction(Id, policy!.DistrictId!, due, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}, Tax Id: {policy.Id}, Tax Type: {policy.taxType.ToString()}");
            taxtrans.NonAsyncExecute(true);
        }
        */
    }
}