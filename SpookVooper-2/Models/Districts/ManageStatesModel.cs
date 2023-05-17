namespace SV2.Models.Districts;
public class ManageStatesModel {
    public List<State> States { get; set; }
    public District District { get; set; }

    public CreateStateModel CreateStateModel { get; set; }
}