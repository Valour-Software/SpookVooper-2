using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using SV2.Database.Models.Permissions;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Models.Leaderboard;
using Microsoft.EntityFrameworkCore;

namespace SV2.Controllers;

public class LeaderboardController : Controller
{
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(ILogger<LeaderboardController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index(int id)
    {
        var model = new LeaderboardIndexModel()
        {
            Users = DBCache.GetAll<SVUser>().OrderByDescending(x => x.Xp).ToList(),
            Page = id,
            Amount = 25
        };

        return View(model);
    }
}