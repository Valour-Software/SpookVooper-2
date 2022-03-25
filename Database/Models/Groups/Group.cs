using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Groups;

public enum GroupType
{
    Company,
    Corporation,
    NonProfit,
    PoliticalParty, 
    District
}

public enum GroupFlag
{
    // is only set by the CFV
    Charity,
    // is only set by the MOJ
    News
}

public class Group : Entity
{
    [JsonIgnore]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
    // used for tax purposes
    public decimal CreditsYesterday { get; set;}
    public GroupType groupType { get; set;}
    // will be use the PostgreSQL Array datatype
    public List<GroupFlag> Flags { get; set;}
    // if the group is open to the public
    public bool Open { get; set;}

}