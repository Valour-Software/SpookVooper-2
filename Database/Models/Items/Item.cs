using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

public class TradeItem :  IHasOwner
{
    [Key]
    public string Id { get; set; }
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { get; set; }
    public string Definition_Id { get; set; }
    [ForeignKey("DefinitionId")]
    public TradeItemDefinition Definition { get; set; }
    public decimal Amount { get; set;}
    // json list of modifiers
    public string Modifiers { get; set; }
}

public class TradeItemDefinition : IHasOwner
{
    public string Id { get; set; }
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { get; set; }
    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; }
}