using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;
using SV2.Workers;
using SV2.Database.Models.Items;
using SV2.Web;

public class ItemTrade
{
    [Key]
    [GuidID]
    public string Id { get; set; }
    public int Amount { get; set; }
    
    [NotMapped]
    public TradeItemDefinition Definition {
        get {
            return DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Id == Definition_Id)!;
        }
    }
    public string Definition_Id { get; set; }

    public DateTime Time { get; set; }

    [EntityId]
    public string FromId { get; set; }

    [EntityId]
    public string ToId { get; set; }

    [VarChar(1024)]
    public string Details { get; set; }

    [NotMapped]

    public bool IsCompleted = false;

    [NotMapped]

    public TaskResult? Result = null;

    [NotMapped]

    public bool Force = false;

    public ItemTrade()
    {
        
    }

    public ItemTrade(string fromId, string toId, int amount, string definition_id, string details)
    {
        Id = Guid.NewGuid().ToString();
        Amount = amount;
        FromId = fromId;
        ToId = toId;
        Time = DateTime.UtcNow;
        Definition_Id = definition_id;
        Details = details;
    }

    public async Task<TaskResult> Execute(bool force = false)
    {
        Force = force;
        ItemTradeManager.itemTradeQueue.Enqueue(this);

        while (!IsCompleted) await Task.Delay(1);

        return Result!;
    }

    public void NonAsyncExecute(bool force = false)
    {
        Force = force;
        ItemTradeManager.itemTradeQueue.Enqueue(this);
    }

    public async Task<TaskResult> ExecuteFromManager(bool Force = false)
    {

        while (TransactionManager.ActiveSvids.Contains(FromId) || TransactionManager.ActiveSvids.Contains(ToId))
        {
            await Task.Delay(1);
        }

        if (!Force && Amount < 0)
        {
            return new TaskResult(false, "Amount must be positive.");
        }
        if (Amount == 0)
        {
            return new TaskResult(false, "Amount must be above 0");
        }

        IEntity? fromEntity = IEntity.Find(FromId);
        IEntity? toEntity = IEntity.Find(ToId);

        if (fromEntity == null) { return new TaskResult(false, $"Failed to find sender {FromId}."); }
        if (toEntity == null) { return new TaskResult(false, $"Failed to find reciever {ToId}."); }

        TradeItem? fromitem = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.OwnerId == FromId && x.Definition_Id == Definition_Id);
        TradeItem? toitem = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.OwnerId == ToId && x.Definition_Id == Definition_Id);

        if (fromitem is null) {
            return new TaskResult(false, $"{fromEntity.Name} lacks any {Definition.Name} to give {Amount} to ¢{toEntity.Name}");
        }

        if (!Force && fromitem.Amount < Amount)
        {
            return new TaskResult(false, $"{fromEntity.Name} lacks the enough of {Definition.Name} to give {Amount} to ¢{toEntity.Name}");
        }

        ItemTradeManager.ActiveSvids.Add(FromId);
        ItemTradeManager.ActiveSvids.Add(ToId);

        // check if the entity we are sending already has this TradeItem        
        // if null then create one

        if (toitem is null) 
        {
            toitem = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ToId,
                Definition_Id = Definition_Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(toitem.Id, toitem);
            await VooperDB.Instance.TradeItems.AddAsync(toitem);
            await VooperDB.Instance.SaveChangesAsync();
        }

        // do tariffs

        if (ResourceManager.Resources.Contains(toitem.Definition.Name) && toitem.Definition.OwnerId == "g-vooperia")
        {
            TaxPolicy? FromDistrictTaxPolicy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == fromEntity.DistrictId & (x.taxType == TaxType.ImportTariff || x.taxType == TaxType.ExportTariff));
            TaxPolicy? ToDistrictTaxPolicy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == toEntity.DistrictId & (x.taxType == TaxType.ImportTariff || x.taxType == TaxType.ExportTariff));

            // fun fact, the entity IRL that imports or exports pays the tariff

            if (FromDistrictTaxPolicy is not null) {
                decimal taxamount = FromDistrictTaxPolicy.GetTaxAmountForResource((decimal)Amount);
                string detail = $"Tax payment for item id: {Id}, Tax Id: {FromDistrictTaxPolicy.Id}, Tax Type: {FromDistrictTaxPolicy.taxType.ToString()}";
                Transaction tran = new Transaction(FromId, FromDistrictTaxPolicy!.DistrictId!, taxamount, TransactionType.TaxPayment, detail);
                tran.Execute(true);
            }
            if (ToDistrictTaxPolicy is not null) {
                decimal taxamount = ToDistrictTaxPolicy.GetTaxAmountForResource((decimal)Amount);
                string detail = $"Tax payment for item trade id: {Id}, Tax Id: {ToDistrictTaxPolicy.Id}, Tax Type: {ToDistrictTaxPolicy.taxType.ToString()}";
                Transaction tran = new Transaction(FromId, ToDistrictTaxPolicy!.DistrictId!, taxamount, TransactionType.TaxPayment, detail);
                tran.Execute(true);
            }
        }
        
        toitem.Amount += Amount;
        fromitem.Amount -= Amount;

        VooperDB.Instance.ItemTrades.AddAsync(this);

        ItemTradeManager.ActiveSvids.Remove(FromId);
        ItemTradeManager.ActiveSvids.Remove(ToId);

        return new TaskResult(true, $"Successfully gave {Amount} of {toitem.Definition.Name} to {toEntity!.Name}.");
    }
}