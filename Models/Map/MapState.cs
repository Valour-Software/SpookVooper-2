namespace SV2.Models.Map;

public class MapState
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public District District => DBCache.Get<District>(DistrictId);

    public string D { get; set; }
    public bool IsOcean { get; set; }
}
