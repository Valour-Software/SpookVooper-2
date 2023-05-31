using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using System.Diagnostics;
using SV2.Models.Manage;
using Valour.Shared.Models;
using SV2.VoopAI;
using Valour.Shared.Authorization;
using Valour.Api.Client;
using System.Web;
using System.Text.Json;
using SV2.Helpers;
using Valour.Api.Nodes;
using SV2.NonDBO;
using Valour.Shared;

namespace SV2.Controllers
{
    public class AccountController : SVController {
        private static List<string> OAuthStates = new();

#if DEBUG
        private static string Redirecturl = "https://localhost:7186/callback";
#else
        private static string Redirecturl = "https://spookvooper.com/callback";
#endif
        private readonly ILogger<AccountController> _logger;
        
        [TempData]
        public string StatusMessage { get; set; }

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Manage()
        {
            SVUser? user = UserManager.GetUser(HttpContext);
            UserManageModel userManageModel = new()
            {
                Id = user.Id,
                Name = user.Name,
                user = user
            };

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View(userManageModel);
        }

        public async Task<IActionResult> ViewAPIKey()
        {
            SVUser? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View((object)user.ApiKey);
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("svid");
            return Redirect("/");
        }

        public IActionResult Entered()
        {
            
            long svid = UserManager.GetSvidFromSession(HttpContext);
            Console.WriteLine(HttpContext.Session.GetString("code"));
            HttpContext.Response.Cookies.Append("svid", svid.ToString());
            return Redirect("/");
        }

        public static List<string> NodeNames = new()
        {
            "emma",
            "jeff"
        };

        [Route("/callback")]
        public async Task<IActionResult> Callback(string code, string state, string node = "")
        {
            if (!OAuthStates.Contains(state))
                return Forbid();

            var url = $"api/oauth/token?client_id={ValourConfig.instance.OAuthClientId}&client_secret={ValourConfig.instance.OAuthClientSecret}&grant_type=authorization_code&code={code}&redirect_uri={HttpUtility.UrlEncode(Redirecturl)}&state={state}";

            TaskResult<Valour.Api.Models.AuthToken> result;
            if (node != "")
                result = await ValourClient.GetJsonAsync<Valour.Api.Models.AuthToken>(url, http: NodeManager.GetNodeFromName(node)?.HttpClient);
            else
            {
                result = new();
                foreach (var name in NodeNames)
                {
                    result = await ValourClient.GetJsonAsync<Valour.Api.Models.AuthToken>(url, http: NodeManager.GetNodeFromName(name)?.HttpClient);
                    if (result.Success)
                        break;
                }
            }
            //Console.WriteLine(result.Data);
            if (!result.Success)
                Console.WriteLine(result.Message);
            var token = result.Data;
            var valouruser = await Valour.Api.Models.User.FindAsync(token.UserId);

            var user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.ValourId == token.UserId);
            if (user is null)
            {
                using var dbctx = VooperDB.DbFactory.CreateDbContext();
                user = new SVUser(valouruser.Name, valouruser.Id);
                DBCache.AddNew(user.Id, user);

                //await dbctx.SaveChangesAsync();
            }

            user.ImageUrl = valouruser.PfpUrl;
            user.OAuthToken = token.Id;
            await user.Create();

            HttpContext.Response.Cookies.Append("svid", user.Id.ToString());
            return Redirect("/");
        }

        public async Task<IActionResult> Login()
        {
            var oauthstate = Guid.NewGuid().ToString();

            AuthorizeModel model = new()
            {
                ClientId = ValourConfig.instance.OAuthClientId,
                RedirectUri = HttpUtility.UrlEncode(Redirecturl),
                UserId = ValourNetClient.BotId,
                ResponseType = "",
                Scope = UserPermissions.Minimum.Value | UserPermissions.View.Value | UserPermissions.EconomyViewPlanet.Value | UserPermissions.EconomySendPlanet.Value,
                Code = "",
                State = oauthstate
            };

            string url = $"https://app.valour.gg/authorize?client_id={ValourConfig.instance.OAuthClientId}";
            url += $"&response_type=code&redirect_uri={HttpUtility.UrlEncode(Redirecturl)}&state={oauthstate}&scope={model.Scope}";
            OAuthStates.Add(oauthstate);
            return Redirect(url);
            //Console.WriteLine(oauthstate);

            // var result = await ValourClient.PostAsyncWithResponse<string>($"api/oauth/authorize", model);
            // if (!result.Success)
            //     Console.WriteLine(result.Message);
            //var url = result.Data;
            // OAuthStates.Add(oauthstate);
            //return Redirect(url);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}