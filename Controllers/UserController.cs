using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using System.Diagnostics;

namespace SV2.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        
        [TempData]
        public string StatusMessage { get; set; }

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public IActionResult Entered()
        {
            string svid = UserManager.GetSvidFromSession(HttpContext);
            Console.WriteLine(HttpContext.Session.GetString("code"));
            HttpContext.Session.SetString("svid", svid);
            return View((object)svid);
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