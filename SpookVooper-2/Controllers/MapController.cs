using Microsoft.AspNetCore.Mvc;
using SV2.Models.Map;
using System.Text.Json.Serialization;
using System.Xml;
using System.Text;
using System.Text.Json;
using SV2.NonDBO;
using IdGen;
using SV2.Database.Managers;
using SV2.Helpers;

[ApiExplorerSettings(IgnoreApi = true)]
public class MapController : SVController {
    private readonly ILogger<MapController> _logger;

    public static List<MapState> MapStates = new List<MapState>();

    public static List<DistrictMap> DistrictMaps = new();

    [TempData]
    public string StatusMessage { get; set; }

    public MapController(ILogger<MapController> logger)
    {
        _logger = logger;
    }

    public IActionResult World()
    {
        return View(MapStates);
    }
}