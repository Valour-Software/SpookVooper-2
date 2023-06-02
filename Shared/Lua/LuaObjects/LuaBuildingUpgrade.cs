using Shared.Lua.Scripting;

namespace Shared.Lua.LuaObjects;

public class LuaBuildingUpgrade : Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DictNode Costs { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }

    public static async ValueTask<LuaBuildingUpgrade> FindAsync(string id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<LuaBuildingUpgrade>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<LuaBuildingUpgrade>($"api/lua/luabuildingupgrades/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}
