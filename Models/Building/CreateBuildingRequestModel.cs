using Microsoft.AspNetCore.Mvc.Rendering;
using SV2.Scripting.LuaObjects;

namespace SV2.Models.Building;

public class CreateBuildingRequestModel 
{
    public Province Province { get; set; }
    public LuaBuilding LuaBuildingObj { get; set; }
    public long RequesterId { get; set; }
    public string BuildingId { get; set; }
    public long? AlreadyExistingBuildingId { get; set; }
    public long ProvinceId { get; set; }
    public int levelsToBuild { get; set; }
    public List<SelectListItem> CanBuildAs { get; set; }
    public long BuildAsId { get; set; }

    public List<BuildingRequest> CurrentRequestsFromThisUser { get; set; }
}