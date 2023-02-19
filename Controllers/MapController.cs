using Microsoft.AspNetCore.Mvc;
using SV2.Models.Map;
using System.Text.Json.Serialization;
using System.Xml;
using System.Text;
using System.Text.Json;
using SV2.NonDBO;
using IdGen;

public class MapController : Controller
{
    private readonly ILogger<MapController> _logger;

    private static List<MapState> MapStates = new List<MapState>();

    public static List<DistrictMap> DistrictMaps = new();

    [TempData]
    public string StatusMessage { get; set; }

    public MapController(ILogger<MapController> logger)
    {
        _logger = logger;
    }

    public static void LoadMap()
    {
        using var dbctx = VooperDB.DbFactory.CreateDbContext();
        string data = System.IO.File.ReadAllText("Managers/Data/dystopia.json");
        var mapdata = JsonSerializer.Deserialize<MapDataJson>(data);

        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(System.IO.File.ReadAllText("Managers/Data/mapfromtool.svg"));
        List<MapState> mapStates = new();

        var n = doc.ChildNodes.Item(0);
        foreach (var node in doc.ChildNodes)
        {
            if (((XmlNode)node).Name == "svg")
            {
                foreach (var _child in ((XmlNode)node).ChildNodes)
                {
                    var child = (XmlNode)_child;
                    if (child.Name == "path")
                    {
                        var districtname = child.Attributes["id"].Value.Replace("_", " ");
                        if (!(child.Name == "path"))
                            continue;
                        long id = long.Parse(child.Attributes["id"].Value);
                        var district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name == mapdata.Data.FirstOrDefault(x => x.Value.Contains(id)).Key);
                        long disid = 100;
                        if (district is not null)
                            disid = district.Id;
                        var state = new MapState()
                        {
                            Id = id,
                            D = child.Attributes["d"].Value,
                            DistrictId = disid,
                            IsOcean = false
                        };
                        mapStates.Add(state);
                    }
                }
            }
        }

        var provincestringdata = System.IO.File.ReadAllText("Managers/Data/definition.csv");
        foreach (var line in provincestringdata.Split('\n'))
        {
            if (line.Contains("sea"))
            {
                long id = long.Parse(line.Split(";")[0]);
                var state = mapStates.FirstOrDefault(x => x.Id == id);
                if (state is not null)
                    state.IsOcean = true;
            }
        }

        var _mapStates = new List<MapState>();
        foreach (var state in mapStates)
        {
            if (state.DistrictId == 100 || state.IsOcean == true)
                continue;

            var districtstate = _mapStates.FirstOrDefault(x => x.DistrictId == state.DistrictId);
            var districtmapdata = DistrictMaps.FirstOrDefault(x => x.DistrictId == state.DistrictId);
            if (districtstate is not null)
            {
                districtmapdata.Provinces.Add(state);
                districtstate.D += $" {state.D}";
                var posinfo = state.D.Split(" ");
                int xpos = (int)double.Parse(posinfo[1]);
                int ypos = (int)double.Parse(posinfo[2]);
                state.XPos = xpos;
                state.YPos = ypos;

                if (districtmapdata.LowestXPos > xpos)
                    districtmapdata.LowestXPos = xpos;
                if (districtmapdata.LowestYPos > ypos)
                    districtmapdata.LowestYPos = ypos;
                if (districtmapdata.HighestXPos < xpos)
                    districtmapdata.HighestXPos = xpos;
                if (districtmapdata.HighestYPos < ypos)
                    districtmapdata.HighestYPos = ypos;
            }
            else
            {
                districtstate = new MapState()
                {
                    Id = state.DistrictId,
                    D = state.D,
                    DistrictId = state.DistrictId,
                    IsOcean = false
                };
                _mapStates.Add(districtstate);

                districtmapdata = new()
                {
                    Provinces = new(),
                    DistrictId = state.DistrictId,
                    LowestXPos = 9999,
                    LowestYPos = 9999,
                    HighestYPos = 0,
                    HighestXPos = 0,
                };

                districtmapdata.Provinces.Add(state);

                DistrictMaps.Add(districtmapdata);
            }

            var dbprovince = DBCache.Get<Province>(state.Id);
            if (dbprovince is null)
            {
                var district = DBCache.Get<District>(state.DistrictId);
                dbprovince = new()
                {
                    DistrictId = state.DistrictId,
                    Id = state.Id,
                    Name = $"Province {state.Id}"
                };
                DBCache.Put(dbprovince.Id, dbprovince);
                dbctx.Provinces.Add(dbprovince);
                //district.Provinces.Add(dbprovince);
            }
            else
            {
                dbprovince.DistrictId = districtstate.DistrictId;
                dbprovince.District = districtstate.District;
            }
        }
        dbctx.SaveChanges();

        MapStates = _mapStates;
    }

    public IActionResult World()
    {
        return View(MapStates);
    }
}