using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Groups;

public enum GroupType
{
    Company,
    // a corporation is a company that is listed on SVSE or a company on a private stock exchange that the CFV has determined is a corporation
    Corporation,
    NonProfit,
    PoliticalParty, 
    District
}

public enum GroupFlag
{
    // is only given by the CFV
    Charity,
    // is only given by the MOJ
    News
}

public class Group : IHasOwner, IEntity
{
    [Key]
    public string Id { get; set; }
    public string Name { get; }
    public string Description { get; set; }
    public string Image_Url { get; set; }
    public decimal Credits { get; set;}
    public decimal CreditsYesterday { get; set;}
    [JsonIgnore]
    public string Api_Key { get; set; }
    public GroupType groupType { get; set; }
    // will be use the PostgreSQL Array datatype
    public List<GroupFlag> Flags { get; set; }
    // if the group is open to the public
    public bool Open { get; set; }
    public string Owner_Id { get; set; }
}