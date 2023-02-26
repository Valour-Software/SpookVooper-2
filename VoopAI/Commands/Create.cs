using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;
using Valour.Api.Models.Messages.Embeds;

namespace SV2.VoopAI.Commands;

public class CreateCommands : CommandModuleBase
{
    [Group("info")]
    public class InfoGroup : CommandModuleBase
    {
        [Command("xp")]
        public Task XpInfo(CommandContext ctx)
        {
            var embed = new EmbedBuilder().AddPage().AddRow()
                .AddText("Message Xp", "The more chars (numbers, letters, etc) you type in a given minute, the more xp you earn. However, each additional char adds a little less xp.").AddRow()
                .AddText("Element Xp", "By combining elements, you will earn xp depending on how difficult the combination was.");
            return ctx.ReplyAsync(embed);
        }

        [Command("elements")]
        public Task ElementsInfo(CommandContext ctx)
        {
            var embed = new EmbedBuilder().AddPage().AddRow()
                .AddText("gg5", "323232");
            return ctx.ReplyAsync(embed);
        }
    }

    [Group("create")]
    public class CreateGroup : CommandModuleBase
    {
        [Command("account")]
        public async Task _CreateAccount(CommandContext ctx)
        {
            SVUser? _user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
            if (_user is not null)
            {
                await ctx.ReplyAsync("You already have a SV account!");
                return;
            }
            using var dbctx = VooperDB.DbFactory.CreateDbContext();

            SVUser user = new SVUser(ctx.Member.Nickname, ctx.Member.UserId);
            user.ImageUrl = (await ctx.Member.GetUserAsync()).PfpUrl;
            DBCache.Put(user.Id, user);

            DBCache.dbctx.Users.Add(user);
            //await dbctx.SaveChangesAsync();
            await ctx.ReplyAsync("Successfully created SV account.");
        }
    }

    [Command("create")]
    public async Task GetInfoAsync(CommandContext ctx)
    {
        ctx.ReplyAsync("what");
    }
}