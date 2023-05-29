namespace SV2.Models.Government;

public class GovernmentIndexModel
{
    public SVUser? Emperor;
    public SVUser? PrimeMinister;
    public SVUser? CFV;
    public List<Senator> Senators;
    public List<SVUser> Justices;
    public List<SVUser> PanelMembers;
}