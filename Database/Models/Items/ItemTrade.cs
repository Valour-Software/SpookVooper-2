using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;
using SV2.Workers;
using SV2.Database.Models.Items;
using SV2.Web;
using SV2.Managers;

public enum ItemTradeType 
{
    Normal = 0,
    Server = 1
}

public class ItemTrade
{
    [Key]
    public long Id {get; set; }
    public double Amount { get; set; }
    
    [NotMapped]
    public ItemDefinition Definition => DBCache.Get<ItemDefinition>(DefinitionId)!;
    public long DefinitionId { get; set; }

    public DateTime Time { get; set; }

    public long? FromId { get; set; }

    public long ToId { get; set; }

    [VarChar(1024)]
    public string Details { get; set; }

    public ItemTradeType TradeType { get; set; }

    [NotMapped]

    public bool IsCompleted = false;

    [NotMapped]

    public TaskResult? Result = null;

    [NotMapped]

    public bool Force = false;

    public ItemTrade()
    {
        
    }

    public ItemTrade(ItemTradeType tradetype, long? fromId, long toId, double amount, long definitionid, string details)
    {
        Id = IdManagers.GeneralIdGenerator.Generate();
        Amount = amount;
        FromId = fromId;
        ToId = toId;
        Time = DateTime.UtcNow;
        DefinitionId = definitionid;
        Details = details;
        TradeType = tradetype;
    }

    public async Task<TaskResult> Execute(bool force = false)
    {
        Force = force;
        ItemTradeManager.itemTradeQueue.Enqueue(this);

        while (!IsCompleted) await Task.Delay(5);

        return Result!;
    }

    public void NonAsyncExecute(bool force = false)
    {
        Force = force;
        ItemTradeManager.itemTradeQueue.Enqueue(this);
    }

    public async Task<TaskResult> ExecuteFromManager(VooperDB dbctx, bool Force = false)
    {
        if (!Force && Amount < 0)
        {
            return new TaskResult(false, "Amount must be positive.");
        }
        if (Amount == 0)
        {
            return new TaskResult(false, "Amount must be above 0");
        }

        BaseEntity? fromEntity = BaseEntity.Find(FromId);
        BaseEntity? toEntity = BaseEntity.Find(ToId);

        if (fromEntity == null && TradeType != ItemTradeType.Server) { return new TaskResult(false, $"Failed to find sender {FromId}."); }
        if (toEntity == null) { return new TaskResult(false, $"Failed to find reciever {ToId}."); }

        SVItemOwnership? fromitem = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.OwnerId == FromId && x.DefinitionId == DefinitionId);
        SVItemOwnership? toitem = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.OwnerId == ToId && x.DefinitionId == DefinitionId);

        if (fromitem is null && TradeType != ItemTradeType.Server) {
            return new TaskResult(false, $"{fromEntity.Name} lacks any {Definition.Name} to give {Amount} to ¢{toEntity.Name}");
        }

        if (!Force && fromitem.Amount < Amount)
        {
            return new TaskResult(false, $"{fromEntity.Name} lacks the enough of {Definition.Name} to give {Amount} to ¢{toEntity.Name}");
        }

        // check if the entity we are sending already has this TradeItem        
        // if null then create one

        if (toitem is null) 
        {
            toitem = new SVItemOwnership() {
                Id = IdManagers.GeneralIdGenerator.Generate(),
                OwnerId = toEntity.Id,
                DefinitionId = DefinitionId,
                Amount = 0
            };
            DBCache.Put(toitem.Id, toitem);
            DBCache.dbctx.SVItemOwnerships.Add(toitem);
            toEntity.SVItemsOwnerships[toitem.DefinitionId] = toitem;
        }

        // do tariffs

        if (!(TradeType == ItemTradeType.Server) && GameDataManager.Resources.ContainsKey(toitem.Definition.Name) && toitem.Definition.IsSVItem)
        {
            TaxPolicy? FromDistrictTaxPolicy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == fromEntity.DistrictId & (x.taxType == TaxType.ImportTariff || x.taxType == TaxType.ExportTariff));
            TaxPolicy? ToDistrictTaxPolicy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == toEntity.DistrictId & (x.taxType == TaxType.ImportTariff || x.taxType == TaxType.ExportTariff));

            // fun fact, the entity IRL that imports or exports pays the tariff

            if (FromDistrictTaxPolicy is not null) {
                decimal taxamount = FromDistrictTaxPolicy.GetTaxAmountForResource((decimal)Amount);
                string detail = $"Tax payment for item id: {Id}, Tax Id: {FromDistrictTaxPolicy.Id}, Tax Type: {FromDistrictTaxPolicy.taxType.ToString()}";
                Transaction tran = new Transaction((long)FromId, FromDistrictTaxPolicy!.DistrictId!, taxamount, TransactionType.TaxPayment, detail);
                tran.Execute(true);
            }
            if (ToDistrictTaxPolicy is not null) {
                decimal taxamount = ToDistrictTaxPolicy.GetTaxAmountForResource((decimal)Amount);
                string detail = $"Tax payment for item trade id: {Id}, Tax Id: {ToDistrictTaxPolicy.Id}, Tax Type: {ToDistrictTaxPolicy.taxType.ToString()}";
                Transaction tran = new Transaction((long)FromId!, ToDistrictTaxPolicy!.DistrictId!, taxamount, TransactionType.TaxPayment, detail);
                tran.Execute(true);
            }
        }
        
        toitem.Amount += Amount;
        if (FromId is not null)
            fromitem.Amount -= Amount;

        VooperDB.Instance.ItemTrades.Add(this);

        return new TaskResult(true, $"Successfully gave {Amount} of {toitem.Definition.Name} to {toEntity!.Name}.");
    }
}