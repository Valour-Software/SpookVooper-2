using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

public class TradeItem
{
    public string Id { get; set;}
    public string Holder_Id { get; set;}
    [NotMapped]
    public Entity Holder { get; set;}
    public string Definition_Id { get; set;}
    [NotMapped]
    public TradeItemDefinition Definition { get; set;}
    public decimal Amount { get; set;}
    // json list of modifiers
    public string Modifiers { get; set;}
}

public class TradeItemDefinition
{
    public string Id { get; set;}
    public string Creator_Id { get; set;}
    [NotMapped]
    public Entity Creator { get; set;}
    // defines if this definition is owned by SV
    public bool SV_Owned { get; set;}
    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    public string Name { get; set;}
    public DateTime Created { get; set;}
}