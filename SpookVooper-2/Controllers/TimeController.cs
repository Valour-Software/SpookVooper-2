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

namespace SV2.Controllers;

public class TimeController : SVController 
{
    private readonly ILogger<TimeController> _logger;
    public TimeController(ILogger<TimeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return View(DBCache.CurrentTime);
    }
}