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
}