using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using System.Diagnostics;

namespace SV2.Controllers
{
    public class GroupController : Controller
    {
        private readonly ILogger<GroupController> _logger;
        
        [TempData]
        public string StatusMessage { get; set; }

        public GroupController(ILogger<GroupController> logger)
        {
            _logger = logger;
        }

        public IActionResult View(string id)
        {
            Group? group = Group.Find(id);
            return View(group);
        }

        public IActionResult Edit(string id)
        {
            Group? group = Group.Find(id);
            return View(group);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}