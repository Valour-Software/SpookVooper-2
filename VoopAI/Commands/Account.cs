using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Items.Messages;
using Valour.Shared.Items.Messages.Embeds;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using System.Linq;
using System.Collections.Concurrent;
using SV2.Web;
using SV2.Managers;

namespace SV2.VoopAI.Commands;

class AccountCommands : CommandModuleBase
{

    public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

    ConcurrentDictionary<ulong, DateTime> LastMinuteTicked = new();

    ConcurrentDictionary<ulong, int> PointsThisMinute = new();

    [Event("Message")]
    public async Task OnMessage(CommandContext ctx)
    {
        User? user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (user is not null)
        {

            if (LastMinuteTicked.ContainsKey(ctx.Member.User_Id)) {
                if (LastMinuteTicked[ctx.Member.User_Id].AddSeconds(60) < DateTime.UtcNow) {
                    double xpgain = (Math.Log10((double)PointsThisMinute[ctx.Member.User_Id]) - 1)*3;
                    xpgain = Math.Max(0.2, xpgain);
                    user.Xp += (float)xpgain;
                    user.MessageXp += (float)xpgain;
                    user.ActiveMinutes += 1;
                    PointsThisMinute[ctx.Member.User_Id] = 0;
                    LastMinuteTicked[ctx.Member.User_Id] = DateTime.UtcNow;
                }
            }
            else {
                LastMinuteTicked.TryAdd(ctx.Member.User_Id, DateTime.UtcNow);
                PointsThisMinute.TryAdd(ctx.Member.User_Id, 0);
            }

            string Content = RemoveWhitespace(ctx.Message.Content);

            Content = Content.Replace("*", "");

            int Points = 0;

            // do char points
            // each char grants 1 point
            Points += Content.Length;

            // if there is media then add 100 points
            if (Content.Contains("https://vmps.valour.gg"))
            {
                Points += 100;
            }

            PointsThisMinute[ctx.Member.User_Id] += Points;
            user.TotalChars += Content.Length;
            user.PointsTotal += Points;

            user.Messages += 1;

            user.Image_Url = (await ctx.Member.GetUserAsync()).Pfp_Url;
        }
    }

    [Command("login")]
    public async Task Login(CommandContext ctx, string code)
    {
        User? user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (user is null)
        {
            user = new User(ctx.Member.Nickname, ctx.Member.User_Id);
            await DBCache.Put<User>(user.Id, user);
            await VooperDB.Instance.Users.AddAsync(user);
            await VooperDB.Instance.SaveChangesAsync();
        }
        UserManager.AddLogin(code, user!.Id);
        await ctx.ReplyAsync("Successfully logged you in! Please go back to the login page and click 'Entered'");
    }

    [Command("svid")]
    public async Task ViewSVID(CommandContext ctx) 
    {
        User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (_user is null)
        {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        await ctx.ReplyAsync(_user.Id);
    }

    [Command("xp")]
    [Alias("do")]
    public async Task ViewXP(CommandContext ctx) 
    {
        User? user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (user is null)
        {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }
        EmbedBuilder builder = new();
        EmbedPageBuilder page = new();
        page.AddText(null, $"{Math.Round(user.Xp,1)} XP {user.Rank.ToString()}");
        page.AddText("Messages", $"{user.Messages}");
        page.AddText("Message To XP Ratio", $"1 : {Math.Round((double)user.MessageXp/(double)user.Messages, 2)}");

        // get daily UBI

        // get vooperia's ubi
        decimal ubi = 0.0m;
        ubi += DBCache.GetAll<UBIPolicy>().FirstOrDefault(x => x.DistrictId == null && x.ApplicableRank == user.Rank)!.Rate;
        
        // get the user's district's UBI
        ubi += DBCache.GetAll<UBIPolicy>().Where(x => x.DistrictId == user.DistrictId && (x.ApplicableRank == user.Rank || x.ApplicableRank == null)).Sum(x => x.Rate);

        page.AddText("Daily UBI", $"¢{Math.Round(ubi)}");
        builder.AddPage(page);
        await ctx.ReplyAsync(builder);
    }
    
    [Command("savedb")]
    public async Task savedb(CommandContext ctx) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        await DBCache.SaveAsync();
    }
}