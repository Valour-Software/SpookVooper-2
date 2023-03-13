using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using SV2.Database.Models.Permissions;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Helpers;

namespace SV2.Controllers;

public class UserController : SVController {
    private readonly ILogger<UserController> _logger;
    
    [TempData]
    public string StatusMessage { get; set; }

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/User/Info/{id}")]
    public async Task<IActionResult> Info(long? id)
    {
        if (id is null)
            return View(null);

        SVUser? user = DBCache.GetAll<SVUser>().FirstOrDefault(x => x.Id == id);

        return View(user);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}