using Shared.Models.Districts;
using Shared.Models.Entities;

namespace Shared.Models.Items;

public class ItemDefinition : Item
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public BaseEntity Owner { get; set; }

    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public Dictionary<ItemModifierType, double>? Modifiers { get; set; }

    /// <summary>
    /// For example, if this was a NVTech Tank, the base item would be the SV Tank item definition
    /// </summary>
    public long? BaseItemDefinitionId { get; set; }

    public bool Transferable { get; set; }

    public bool IsSVItem => OwnerId == 100 || BaseItemDefinitionId is not null;
    public string? BaseItemName { get; set; }

    public async ValueTask<Recipe> GetRecipeAsync()
    {
        if (SVCache.ItemDefIdToRecipe.ContainsKey(Id))
            return SVCache.ItemDefIdToRecipe[Id];
        return null;
    }

    public static async ValueTask<ItemDefinition> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<ItemDefinition>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<ItemDefinition>($"api/itemdefinitions/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}