using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;
using Valour.Api.Models.Messages.Embeds;
using Valour.Api.Models.Messages.Embeds.Styles.Basic;

namespace SV2.VoopAI.Commands;

class GroupCommands : CommandModuleBase
{
    [Command("groups")]
    public async Task GroupsOwned(CommandContext ctx)
    {
        SVUser? _user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == ctx.Member.UserId);
        if (_user is null)
        {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        IEnumerable<Group> groups = DBCache.GetAll<Group>().Where(x => x.OwnerId == _user.Id);

        if (groups.Count() == 0) {
            await ctx.ReplyAsync("You do not own any groups!");
            return;
        }

        var embed = new EmbedBuilder().AddPage().AddRow();

        int i = 0;
        foreach(Group group in groups)
        {
            embed.AddText(group.Name, $"¢{Math.Round(group.Credits, 2)}");
            i += 1;
            if (i >= 3) {
                embed.AddRow();
            }
        }

        await ctx.ReplyAsync(embed);
    }   

    [Group("group")]
    public class GroupCommandGroup : CommandModuleBase
    {
        [Command("pay")]
        public async Task PayUsingGroup(CommandContext ctx)
        {
            var embed = new EmbedBuilder().AddPage().AddRow();
            await ctx.ReplyAsync(embed);
        }

        [Command("roles")]
        public async Task ViewRoles(CommandContext ctx, [Remainder] string groupname)
        {
            Group? group = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
            if (group is null)
            {
                await ctx.ReplyAsync($"Could not find {groupname}!");
                return;
            }

            var embed = new EmbedBuilder().AddPage($"{groupname}'s Roles").AddRow();

            foreach(GroupRole role in DBCache.GetAll<GroupRole>().Where(x => x.GroupId == group.Id)) {
                string extra = "";
                if (role.Salary > 0.0m) {
                    extra += $" (¢{Math.Round(role.Salary*24.0m,2)} daily)";
                }
                embed.AddText(text:role.Name+extra).WithStyles(new TextColor(role.Color));
                string members = "";
                foreach(long userid in role.MembersIds) {
                    members += $"{DBCache.Get<SVUser>(userid).Name}, ";
                }
                if (members != "") {
                    members = members.Substring(0, members.Length-2);
                }
                embed.AddText(null, members);
            }
            
            await ctx.ReplyAsync(embed);
        }
    }
}