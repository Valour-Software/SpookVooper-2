namespace SV2.Models.Map;

public class MapState
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public District District => DBCache.Get<District>(DistrictId);

    public string D { get; set; }
    public bool IsOcean { get; set; }

    public int XPos { get; set; }
    public int YPos { get; set; }
}

public class DistrictMap
{
    public long DistrictId { get; set; }
    public int LowestXPos { get; set; }
    public int LowestYPos { get; set; }
    public int HighestXPos { get; set; }
    public int HighestYPos { get; set; }
    public List<MapState> Provinces { get; set; }
}