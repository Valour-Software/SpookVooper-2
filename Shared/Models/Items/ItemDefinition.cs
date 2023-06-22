using Shared.Models.Entities;

namespace Shared.Models.Items;

public enum ItemModifierTypes {
    Attack = 0
}

public class ItemModifier 
{
    public ItemModifierTypes Type { get; set; }
    public double Amount { get; set; }
}

public class ItemDefinition : Item
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public BaseEntity Owner { get; set; }

    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public List<ItemModifier>? Modifiers { get; set; }

    /// <summary>
    /// For example, if this was a NVTech Tank, the base item would be the SV Tank item definition
    /// </summary>
    public long? BaseItemDefinitionId { get; set; }

    public bool Transferable { get; set; }

    public bool IsSVItem => OwnerId == 100 || BaseItemDefinitionId is not null;

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}