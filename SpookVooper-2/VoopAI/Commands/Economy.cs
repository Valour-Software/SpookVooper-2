using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;
using Valour.Api.Models;
using Valour.Api.Models.Messages.Embeds;

namespace SV2.VoopAI.Commands;

class EconomyCommands : CommandModuleBase
{
    [Command("balance")]
    [Alias("bal")]
    public async Task Balance(CommandContext ctx) 
    {
        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (user is null) {
            await ctx.ReplyAsync("You do not have a SV account!");
            return;
        }
        await ctx.ReplyAsync($"{ctx.Member.Nickname}'s balance: ¢{Math.Round(await user.GetCreditsAsync(), 2)}");
    }

    [Command("balance")]
    [Alias("bal")]
    public async Task Balance(CommandContext ctx, [Remainder] string entityname)
    {
        BaseEntity? entity = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.Name == entityname);
        if (entity is null)
            entity = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == entityname);
        if (entity is null)
        {
            await ctx.ReplyAsync($"Could not find entity (group or user) with name: {entityname}");
            return;
        }
        await ctx.ReplyAsync($"{entityname}'s balance: ¢{Math.Round(await entity.GetCreditsAsync(), 2)}");
    }

    [Command("budget")]
    public async Task SeeDistrictTaxInfo(CommandContext ctx, [Remainder] string districtname)
    {
        var district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name.ToLower() == districtname.ToLower());
        if (district is null)
        {
            await ctx.ReplyAsync($"Could not find district with name: {districtname}");
            return;
        }

        var buildings = DBCache.GetAllProducingBuildings().Where(x => x.DistrictId == district.Id && x.BuildingType != BuildingType.Infrastructure && x.OwnerId != district.GroupId);

        var embed = new EmbedBuilder()
            .AddPage($"{district.Name} Income (monthly)")
                .AddRow()
                    .AddText("Buildings", $"{buildings.Count():n0}")
                    .AddText("Total Building Levels", $"{buildings.Sum(x => x.Size):n0}")
                .AddRow()
                    .AddText("Base Property Taxes", $"¢{buildings.Sum(x => district.BasePropertyTax ?? 0):n0}")
                .AddRow()
                    .AddText("Per Level Property Taxes", $"¢{buildings.Sum(x => district.PropertyTaxPerSize * x.Size * x.GetThroughputFromUpgrades() ?? 0):n0}");

        await ctx.ReplyAsync(embed);
    }

    [Command("addmoney")]
    public async Task CreateAccount(CommandContext ctx, decimal amount) 
    {
        if (ctx.Member.UserId != 12201879245422592) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (user is not null) {
            await user.SetCreditsAsync(await user.GetCreditsAsync() + amount);
        }
        await ctx.ReplyAsync($"Added ¢{amount} to Jacob's balance.");
    }

    [Command("pay")]
    public async Task Pay(CommandContext ctx, decimal amount, PlanetMember member) 
    {
        SVUser? from = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (from is null) {
            await ctx.ReplyAsync("You do not have a SV account! Login into SV to create one, https://spookvooper.com");
            return;
        }

        SVUser? to = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == member.UserId);
        if (from is null) {
            await ctx.ReplyAsync("The user you are trying to send credits to lacks a SV account!");
            return;
        }
        var tran = new SVTransaction(from, to, amount, TransactionType.Payment, "Payment from Valour");
        await ctx.ReplyAsync((await tran.Execute()).Info);
    }

    [Command("pay")]
    public async Task Pay(CommandContext ctx, decimal amount, PlanetMember member, [Remainder] string groupname) 
    {
        SVUser? fromuser = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (fromuser is null) {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        Group? from = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
        if (from is null) {
            await ctx.ReplyAsync($"Could not find {groupname}");
            return;
        }

        if (!from.HasPermission(fromuser, GroupPermissions.Eco)) {
            await ctx.ReplyAsync($"You lack permission to send credits using this group!");
            return;
        }

        SVUser? to = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == member.UserId);
        if (to is null) {
            await ctx.ReplyAsync("The user you are trying to send credits to lacks a SV account!");
            return;
        }
        var tran = new SVTransaction(from, to, amount, TransactionType.Payment, "Payment from Valour");
        await ctx.ReplyAsync((await tran.Execute()).Info);
    }

    [Command("pay")]
    public async Task Pay(CommandContext ctx, decimal amount, [Remainder] string groupname) 
    {
        SVUser? fromuser = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (fromuser is null) {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        Group? to = null;
        SVTransaction transaction = null;

        if (groupname.Contains(",")) {
            string[] splited = groupname.Split(",");
            to = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == splited[0]);
            if (to is null) {
                await ctx.ReplyAsync($"Could not find {splited[0]}");
                return;
            }
            Group? from = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == splited[1] || (splited[1][0] == ' ' && x.Name == splited[1].Substring(1, splited[1].Length-1)));
            if (from is null) {
                await ctx.ReplyAsync($"Could not find {splited[1]}");
                return;
            }

            if (!from.HasPermission(fromuser, GroupPermissions.Eco)) {
                await ctx.ReplyAsync($"You lack permission to send credits using this group!");
                return;
            }
            transaction = new SVTransaction(from, to, amount, TransactionType.Payment, "Payment from Valour");
            await ctx.ReplyAsync((await transaction.Execute()).Info);
            return;
        }

        to = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
        if (to is null) {
            await ctx.ReplyAsync($"Could not find {groupname}");
            return;
        }
        transaction = new SVTransaction(fromuser, to, amount, TransactionType.Payment, "Payment from Valour");
        await ctx.ReplyAsync((await transaction.Execute()).Info);
    }

    [Command("forceubiupdate")]
    public async Task forceubiupdate(CommandContext ctx) 
    {
        if (ctx.Member.UserId != 12201879245422592) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        List<UBIPolicy>? UBIPolicies = DBCache.GetAll<UBIPolicy>().ToList();

        foreach(UBIPolicy policy in UBIPolicies) {
            var effected = DBCache.GetAll<SVUser>().ToList();
            long fromId = 100;
            if (policy.DistrictId != 100) {
                effected = effected.Where(x => x.DistrictId == policy.DistrictId).ToList();
                fromId = policy.DistrictId;
            }
            if (policy.ApplicableRank != null) {
                effected = effected.Where(x => x.Rank == policy.ApplicableRank).ToList();
            }
            foreach(var user in effected) {
                var tran = new SVTransaction(BaseEntity.Find(fromId), BaseEntity.Find(user.Id), policy.Rate/24.0m, TransactionType.Paycheck, $"UBI for rank {policy.ApplicableRank.ToString()}");
                TaskResult result = await tran.Execute();
                if (!result.Succeeded) {
                    // no sense to keep paying these members since the group has ran out of credits
                    break;
                }
            }
        }

        await ctx.ReplyAsync("Forced UBI Payout!");
    }
}