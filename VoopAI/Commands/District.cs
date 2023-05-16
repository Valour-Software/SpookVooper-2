using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Models.Messages;
using Valour.Api.Models.Messages.Embeds;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Districts;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;

namespace SV2.VoopAI.Commands;

class DistrictCommands : CommandModuleBase
{
    [Group("district")]
    public class DistrictGroup : CommandModuleBase
    {

        
    }
}