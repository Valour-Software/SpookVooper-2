using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using SV2.Database.Models.Permissions;
using System.Diagnostics;
using SV2.Models.Government;
using Microsoft.EntityFrameworkCore;

namespace SV2.Controllers;

public class GovernmentController : Controller
{
    private readonly ILogger<GovernmentController> _logger;
    
    [TempData]
    public string StatusMessage { get; set; }

    public GovernmentController(ILogger<GovernmentController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        GovernmentIndexModel model = new GovernmentIndexModel();

        using var dbctx = VooperDB.DbFactory.CreateDbContext();
        model.Emperor = await dbctx.Users.FirstOrDefaultAsync(x => x.ValourId == 12200448886571008);
        //model.Chancellor = await dbctx.Users.FirstOrDefaultAsync(x => x.ValourId == );
        model.CFV = await dbctx.Users.FirstOrDefaultAsync(x => x.ValourId == 12201879245422592);
        model.Justices = new();
        model.Senators = await dbctx.Senators.Include(x => x.User).Include(x => x.District).ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> Map()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}