using SV2.Database.Models.Entities;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Items;
using System.Collections.Concurrent;


namespace SV2.Database;

public static class DBCache
{
    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static Dictionary<Type, ConcurrentDictionary<string, object>> HCache = new();

    public static List<T>? GetAll<T>() where T : class
    {   
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
        {
            return new List<T>();
        }
        List<T> list = new();
        foreach (T item in HCache[type].Values) {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Returns true if the cache contains the item
    /// </summary>
    public static bool Contains<T>(string Id) where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
            return false;

        return HCache[type].ContainsKey(Id);
    }

    /// <summary>
    /// Places an item into the cache
    /// </summary>
    public static async Task Put<T>(string Id, T? obj) where T : class
    {
        // Empty object is ignored
        if (obj == null)
            return;

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.Add(type, new ConcurrentDictionary<string, object>());

        if (!HCache[type].ContainsKey(Id)) {
            HCache[type][Id] = obj;
        }
    }

    /// <summary>
    /// Returns the item for the given id, or null if it does not exist
    /// </summary>
    public static T? Get<T>(string Id) where T : class
    {
        var type = typeof(T);

        if (HCache.ContainsKey(type))
            if (HCache[type].ContainsKey(Id)) 
                return HCache[type][Id] as T;

        return null;
    }

    public static async Task<IEntity?> FindEntityAsync(string Id)
    {
        switch (Id.Substring(0, 1))
        {
            case "g":
                if (Contains<Group>(Id)) {
                    return Get<Group>(Id);
                }
                Group group = await VooperDB.Instance.Groups.FindAsync(Id);
                await Put<Group>(Id, group);
                return group;
            case "u":
                if (Contains<User>(Id)) {
                    return Get<User>(Id);
                }
                User user = await VooperDB.Instance.Users.FindAsync(Id);
                await Put<User>(Id, user);;
                return user;
            default:
                return null;
        }
    }

    public static async Task LoadAsync()
    {
        #if !DEBUG

        List<Task> tasks = new();
        foreach(Group group in VooperDB.Instance.Groups) {
            tasks.Add(DBCache.Put<Group>(group.Id, group));
        }
        foreach(User user in VooperDB.Instance.Users) {
            tasks.Add(DBCache.Put<User>(user.Id, user));
        }
        foreach(TaxPolicy policy in VooperDB.Instance.TaxPolicies) {
            tasks.Add(DBCache.Put<TaxPolicy>(policy.Id, policy));
        }
        foreach(TradeItem item in VooperDB.Instance.TradeItems) {
            tasks.Add(DBCache.Put<TradeItem>(item.Id, item));
        }
        foreach(TradeItemDefinition definition in VooperDB.Instance.TradeItemDefinitions) {
            tasks.Add(DBCache.Put<TradeItemDefinition>(definition.Id, definition));
        }
        await Task.WhenAll(tasks);

        #endif
    }

    public static async Task SaveAsync()
    {
        VooperDB.Instance.Groups.UpdateRange(HCache[typeof(Group)].Values as ICollection<Group>);
        VooperDB.Instance.Users.UpdateRange(HCache[typeof(User)].Values as ICollection<User>);
        VooperDB.Instance.TaxPolicies.UpdateRange(HCache[typeof(TaxPolicy)].Values as ICollection<TaxPolicy>);
        VooperDB.Instance.TradeItems.UpdateRange(HCache[typeof(TradeItem)].Values as ICollection<TradeItem>);
        VooperDB.Instance.TradeItemDefinitions.UpdateRange(HCache[typeof(TradeItemDefinition)].Values as ICollection<TradeItemDefinition>);
        await VooperDB.Instance.SaveChangesAsync();
    }
}