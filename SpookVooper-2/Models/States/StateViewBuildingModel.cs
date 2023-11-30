using SV2.Models.Building;

namespace SV2.Models.States;

public class StateViewBuildingModel
{
    public State State { get; set; }
    public List<BuildingManageModel> ManageModels { get; set; }
}
