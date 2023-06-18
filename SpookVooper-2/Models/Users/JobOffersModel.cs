namespace SV2.Models.Users;

public class JobOpening
{
    public ProducingBuilding Building { get; set; }
    public GroupRole Role { get; set; }
    public JobApplication? JobApplication { get; set; }
    public SVUser? User { get; set; }
}

public class JobOpeningsModel
{
    public List<JobOpening> JobOpenings { get; set; }
}
