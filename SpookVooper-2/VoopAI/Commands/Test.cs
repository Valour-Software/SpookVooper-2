using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using IdGen;
using SV2.Database.Models.Items;
using SV2.Managers;

namespace SV2.VoopAI.Commands;

class TestCommands : CommandModuleBase
{
    [Command("ping")]
    public async Task Ping(CommandContext ctx) 
    {
        ctx.ReplyAsync("Pong!");
    }

    [Command("createresource")]
    public async Task CreateResource(CommandContext ctx, string resource, int amount) {
        if (ctx.Member.UserId != 12201879245422592) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        var itemdefid = GameDataManager.ResourcesToItemDefinitions[resource].Id;
        ItemTrade itemtrade = new(ItemTradeType.Server, null, user.Id, amount, itemdefid, "From Valour - /creatresource command");
        itemtrade.NonAsyncExecute(true);
        await ctx.ReplyAsync($"Added {amount} of {resource} to Jacob.");
    }

    [Command("createresource")]
    public async Task CreateResource(CommandContext ctx, int amount, long svid, [Remainder] string resource)
    {
        if (ctx.Member.UserId != 12201879245422592)
        {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        BaseEntity? entity = BaseEntity.Find(svid);
        var itemdefid = GameDataManager.ResourcesToItemDefinitions[resource].Id;
        ItemTrade itemtrade = new(ItemTradeType.Server, null, entity.Id, amount, itemdefid, "From Valour - /creatresource command");
        itemtrade.NonAsyncExecute(true);
        await ctx.ReplyAsync($"Added {amount} of {resource} to {entity.Name}.");
    }

    [Command("minevoucher")]
    public async Task MineVouchers(CommandContext ctx, int amount, long svid)
    {
        if (ctx.Member.UserId != 12201879245422592 && ctx.Member.UserId != 12500452716576768)
        {
            await ctx.ReplyAsync("Only TalkinTurtle can use this command!");
            return;
        }
        BaseEntity? entity = BaseEntity.Find(svid);
        var resources = new Dictionary<string, int>()
        {
            { "steel", 2000 },
            { "simple_components", 2000 },
            { "advanced_components", 200 }
        };

        foreach (var resource in resources)
        {
            var itemdefid = GameDataManager.ResourcesToItemDefinitions[resource.Key].Id;
            ItemTrade itemtrade = new(ItemTradeType.Server, null, entity.Id, resource.Value*amount, itemdefid, "From Valour - /minevoucher command");
            itemtrade.NonAsyncExecute(true);
        }
        await ctx.ReplyAsync($"Gave {amount} mine voucher to {entity.Name}.");
    }

    [Command("givexp")]
    public async Task CreateResource(CommandContext ctx, int amount, long svid)
    {
        if (ctx.Member.UserId != 12201879245422592)
        {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        SVUser? user = SVUser.Find(svid);
        user.MessageXp += (float)amount;
        await ctx.ReplyAsync($"Added {amount}xp to {user.Name}.");
    }
}