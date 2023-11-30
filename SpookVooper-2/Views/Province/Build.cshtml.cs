using Microsoft.AspNetCore.Mvc.Rendering;

namespace SV2.Views.ProvinceViews.Models;

public class SelectBuildingModel {
    public Province Province { get; set; }
    public List<SelectListItem> CanBuildAs { get; set; }
}