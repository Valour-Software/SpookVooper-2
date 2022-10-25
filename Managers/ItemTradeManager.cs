using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using System.Collections.Concurrent;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SV2.Web;

namespace SV2.Managers;

public static class ItemTradeManager
{
    static public HashSet<long> ActiveSvids = new();

    static public ConcurrentQueue<ItemTrade> itemTradeQueue = new();

    static public async Task<bool> Run()
    {
        if (itemTradeQueue.IsEmpty) return false;

        ItemTrade trade;
        bool dequeued = itemTradeQueue.TryDequeue(out trade);

        if (!dequeued) return false;

        TaskResult result = await trade.ExecuteFromManager();

        trade.Result = result;

        trade.IsCompleted = true;

        string success = "SUCC";
        if (!result.Succeeded) success = "FAIL";

        Console.WriteLine($"[{success}] Processed {trade.Details}");

        return true;

        // Notify SignalR
        //string json = JsonConvert.SerializeObject(request);

        //await TransactionHub.Current.Clients.All.SendAsync("NotifyTransaction", json);
    }
}