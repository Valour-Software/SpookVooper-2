using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Entities;

public class TradeItem :  IHasOwner
{
    [Key]
    public string Id { get; set; }
    public string Owner_Id { get; set; }
    public string Definition_Id { get; set; }
    public decimal Amount { get; set;}
    // json list of modifiers
    public string Modifiers { get; set; }
}

public class TradeItemDefinition : IHasOwner
{
    public string Id { get; set; }
    public string Owner_Id { get; set; }
    // defines if this definition is owned by SV
    public bool SV_Owned { get; set; }
    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; }
}