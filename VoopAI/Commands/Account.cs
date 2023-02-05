using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using System.Linq;
using System.Collections.Concurrent;
using SV2.Web;
using SV2.Managers;
using Valour.Api.Models.Messages.Embeds;

namespace SV2.VoopAI.Commands;

public class AccountCommands : CommandModuleBase
{

    public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

    [Event(EventType.Message)]
    public async Task OnMessage(CommandContext ctx)
    {
        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (user is not null)
        {
            user.NewMessage(ctx.Message);

            user.ImageUrl = await ctx.Member.GetPfpUrlAsync();
        }
    }

    [Command("login")]
    public async Task Login(CommandContext ctx, string code)
    {
        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (user is null)
        {
            using var dbctx = VooperDB.DbFactory.CreateDbContext();
            user = new SVUser(ctx.Member.Nickname, ctx.Member.UserId);
            DBCache.Put(user.Id, user);

            dbctx.Users.Add(user);
            await dbctx.SaveChangesAsync();
        }
        UserManager.AddLogin(code, user!.Id);
        await ctx.ReplyAsync("Successfully logged you in! Please go back to the login page and click 'Entered'");
    }

    [Command("svid")]
    public async Task ViewSVID(CommandContext ctx) 
    {
        SVUser? _user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (_user is null)
        {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        await ctx.ReplyAsync(_user.Id.ToString());
    }

    [Command("xp")]
    [Alias("do")]
    public async Task ViewXP(CommandContext ctx) 
    {
        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (user is null)
        {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }
        var embed = new EmbedBuilder()
            .AddPage()
                .AddRow()
                    .AddText(null, $"{Math.Round(user.Xp,1)} XP {user.Rank.ToString()}")
                    .AddText("Messages", $"{user.Messages}")
                    .AddText("Message To XP Ratio", $"1 : {Math.Round((double)user.MessageXp/(double)user.Messages, 2)}");

        // get daily UBI

        // get vooperia's ubi
        decimal ubi = 0.0m;
        ubi += DBCache.GetAll<UBIPolicy>().FirstOrDefault(x => x.DistrictId == 100 && x.ApplicableRank == user.Rank)!.Rate;
        
        // get the user's district's UBI
        ubi += DBCache.GetAll<UBIPolicy>().Where(x => x.DistrictId == user.DistrictId && (x.ApplicableRank == user.Rank || x.ApplicableRank == null)).Sum(x => x.Rate);

        embed.AddText("Daily UBI", $"¢{Math.Round(ubi)}");
        await ctx.ReplyAsync(embed);
    }
    
    [Command("savedb")]
    public async Task savedb(CommandContext ctx) 
    {
        if (ctx.Member.UserId != 12201879245422592) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        await DBCache.SaveAsync();
    }
}