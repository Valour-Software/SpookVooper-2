namespace Shared.Models.Districts;

public class State : Item
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string MapColor { get; set; }
    public long GroupId { get; set; }
    public long DistrictId { get; set; }
    public long? GovernorId { get; set; }

    /// <summary>
    /// Returns the item for the given id
    /// </summary>
    public static async ValueTask<State> FindAsync(long id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<State>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<State>($"api/states/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}