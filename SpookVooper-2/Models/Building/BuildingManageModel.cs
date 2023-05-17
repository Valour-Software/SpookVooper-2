using Microsoft.AspNetCore.Mvc.Rendering;
using SV2.Scripting.LuaObjects;

namespace SV2.Models.Building;

public class BuildingManageModel {
    public ProducingBuilding Building { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public long BuildingId { get; set; }
    public string RecipeId { get; set; }
    public CreateBuildingRequestModel createBuildingRequestModel { get; set; }
}