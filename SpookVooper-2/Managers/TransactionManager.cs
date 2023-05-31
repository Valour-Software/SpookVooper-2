using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Users;
using System.Collections.Concurrent;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SV2.Web;

namespace SV2.Managers;

public static class TransactionManager
{
    static public HashSet<long> ActiveSvids = new();

    static public ConcurrentQueue<SVTransaction> transactionQueue = new();
    static public VooperDB TransactionVooperDB;
    static public DateTime LastTransactionSent = DateTime.UtcNow;
    static public long TransactionsProcessed = 0;


    // TODO: add support for sending bulk transactions
    static public async Task<bool> Run(VooperDB dbctx)
    {
        if (transactionQueue.IsEmpty) return false;

        SVTransaction tran;
        bool dequeued = transactionQueue.TryDequeue(out tran);

        if (!dequeued) return false;

        TaskResult result = await tran.ExecuteFromManager(dbctx);

        tran.Result = result;

        tran.IsCompleted = true;

        string success = "SUCC";
        if (!result.Succeeded) success = "FAIL";

        Console.WriteLine($"[{success}] Processed {tran.Details} for {tran.Credits}. INFO: {result.Info}");

        LastTransactionSent = DateTime.UtcNow;
        TransactionsProcessed += 1;
        if (TransactionsProcessed % 100 == 0)
            Console.WriteLine($"Transactions Processed: {TransactionsProcessed:n0}");
        if (transactionQueue.Count > 10 && TransactionsProcessed%100 == 0)
        {
            Console.WriteLine($"Transaction Queue Length: {transactionQueue.Count}");
        }

        return true;

        // Notify SignalR
        //string json = JsonConvert.SerializeObject(request);

        //await TransactionHub.Current.Clients.All.SendAsync("NotifyTransaction", json);
    }
}