namespace SV2.Models.Map;

public class MapState
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public District District => DBCache.Get<District>(DistrictId);
    public string GetMapColor() {
        var province = DBCache.Get<Province>(Id);
        if (province.StateId is null)
            return District.Color;
        Valour.Api.Models.Messages.Embeds.Styles.Color districtcolor = new(District.Color);
        Valour.Api.Models.Messages.Embeds.Styles.Color statecolor = new(province.State.MapColor);
        Valour.Api.Models.Messages.Embeds.Styles.Color c = new(0,0,0);
        c.Red += (byte)(districtcolor.Red * 0.85);
        c.Green += (byte)(districtcolor.Green * 0.85);
        c.Blue += (byte)(districtcolor.Blue * 0.85);
        c.Red += (byte)(statecolor.Red * 0.85);
        c.Green += (byte)(statecolor.Green * 0.85);
        c.Blue += (byte)(statecolor.Blue * 0.85);
        return $"{c.Red}{c.Green}{c.Blue}";
    }

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