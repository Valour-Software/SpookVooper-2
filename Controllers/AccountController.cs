using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using System.Diagnostics;
using SV2.Models.Manage;

namespace SV2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        
        [TempData]
        public string StatusMessage { get; set; }

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Manage()
        {
            User? user = UserManager.GetUser(HttpContext);
            UserManageModel userManageModel = new()
            {
                Id = user.Id,
                Name = user.Name,
            };

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View(userManageModel);
        }

        public async Task<IActionResult> ViewAPIKey()
        {
            User? user = UserManager.GetUser(HttpContext);

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

        public IActionResult Login()
        {
            string Code = UserManager.GetCode(HttpContext);
            Console.WriteLine(HttpContext.Session.Id);
            return View((Object)Code);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}