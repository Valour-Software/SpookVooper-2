namespace SV2.Models.Groups;
public class EditGroupModel {
    public long Id { get; set; }
    public string Name { get; set; }
    public GroupTypes GroupType { get; set; }
    public string Description { get; set; } 
    public string? ImageUrl { get; set; }
    public long? DistrictId { get; set; }
    public bool Open { get; set; }

    public Group Group { get; set; }
}
