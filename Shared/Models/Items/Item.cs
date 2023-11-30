using Shared.Models.Entities;

namespace Shared.Models.Items;

public class SVItemOwnership : IHasOwner
{
    public long Id {get; set; }
    public long OwnerId { get; set; }
    public BaseEntity Owner { get; set; }
    public long DefinitionId { get; set; }
    public ItemDefinition Definition { get; set; }
    public double Amount { get; set;}
}