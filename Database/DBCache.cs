using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Factories;

namespace SV2.Database;

public static class DBCache
{
    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static Dictionary<Type, ConcurrentDictionary<long, object>> HCache = new();

    public static VooperDB dbctx { get; set; }

    public static IEnumerable<T> GetAll<T>() where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(type))
            yield break;

        foreach (T item in HCache[type].Values)
            yield return item;
    }

    /// <summary>
    /// Places an item into the cache
    /// </summary>
    public static void Remove<T>(long Id) where T : class
    {

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.TryAdd(type, new ConcurrentDictionary<long, object>());

        if (!HCache[type].ContainsKey(Id))
        {
            HCache[type].Remove(Id, out _);
        }
    }

    /// <summary>
    /// Returns true if the cache contains the item
    /// </summary>
    public static bool Contains<T>(long Id) where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
            return false;

        return HCache[type].ContainsKey(Id);
    }

    /// <summary>
    /// Places an item into the cache
    /// </summary>
    public static void Put<T>(long Id, T? obj) where T : class
    {
        // Empty object is ignored
        if (obj == null)
            return;

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.Add(type, new ConcurrentDictionary<long, object>());

        if (!HCache[type].ContainsKey(Id)) {
            HCache[type][Id] = obj;
        }
    }

    /// <summary>
    /// Returns the item for the given id, or null if it does not exist
    /// </summary>
    public static T? Get<T>(long Id) where T : class
    {
        var type = typeof(T);

        if (HCache.ContainsKey(type))
            if (HCache[type].ContainsKey(Id)) 
                return HCache[type][Id] as T;

        return null;
    }

    public static T? Get<T>(long? Id) where T : class
    {
        if (Id is null)
            return null;
        var type = typeof(T);

        if (HCache.ContainsKey(type))
            if (HCache[type].ContainsKey((long)Id)) 
                return HCache[type][(long)Id] as T;

        return null;
    }

    public static BaseEntity? FindEntity(long Id)
    {
        var group = Get<Group>(Id);
        if (group is not null)
            return group;

        var user = Get<SVUser>(Id);
        if (user is not null)
            return user;

        return null;
    }

    public static BaseEntity? FindEntity(long? Id)
    {
        if (Id is null) return null;
        var group = Get<Group>(Id);
        if (group is not null)
            return group;

        var user = Get<SVUser>(Id);
        if (user is not null)
            return user;

        return null;
    }

    public static async Task LoadAsync()
    {
        dbctx = VooperDB.DbFactory.CreateDbContext();
        //#if !DEBUG

        foreach (Group group in dbctx.Groups) {
            Put(group.Id, group);
        }
        foreach(SVUser user in dbctx.Users) {
            Put(user.Id, user);
        }
        foreach(TaxPolicy policy in dbctx.TaxPolicies) {
            Put(policy.Id, policy);
        }
        foreach(TradeItem item in dbctx.TradeItems) {
            Put(item.Id, item);
        }
        foreach(TradeItemDefinition definition in dbctx.TradeItemDefinitions) {
            Put(definition.Id, definition);
        }
        foreach(Factory factory in dbctx.Factories) {
            Put(factory.Id, factory);
        }
        foreach(UBIPolicy policy in dbctx.UBIPolicies) {
            Put(policy.Id, policy);
        }
        foreach(District district in dbctx.Districts) {
            Put(district.Id, district);
        }
        foreach(GroupRole role in dbctx.GroupRoles) {
            Put(role.Id, role);
        }
        foreach(Election election in dbctx.Elections) {
            Put(election.Id, election);
        }
        foreach(Vote vote in dbctx.Votes) {
            Put(vote.Id, vote);
        }
        foreach(Province province in dbctx.Provinces) {
            province.District = Get<District>(province.DistrictId);
            Put(province.Id, province);
        }
        foreach (var _obj in dbctx.Cities)
            Put(_obj.Id, _obj);
        foreach (Recipe recipe in dbctx.Recipes)
            Put(recipe.Id, recipe);
        foreach(Minister minister in dbctx.Ministers)
            Put(minister.UserId, minister);
        foreach (var _obj in dbctx.Senators)
            Put(_obj.DistrictId, _obj);

        foreach (District district in GetAll<District>())
        {
            district.Provinces = GetAll<Province>().Where(x => x.DistrictId == district.Id).ToList();
        }

        //#endif
    }

    public static async Task SaveAsync()
    {
        if (false) {
            dbctx.Groups.UpdateRange(GetAll<Group>());
            dbctx.GroupRoles.UpdateRange(GetAll<GroupRole>());
            dbctx.Users.UpdateRange(GetAll<SVUser>());
            dbctx.TaxPolicies.UpdateRange(GetAll<TaxPolicy>());
            dbctx.TradeItems.UpdateRange(GetAll<TradeItem>());
            dbctx.TradeItemDefinitions.UpdateRange(GetAll<TradeItemDefinition>());
            dbctx.Factories.UpdateRange(GetAll<Factory>());
            dbctx.TaxPolicies.UpdateRange(GetAll<TaxPolicy>());
            dbctx.Districts.UpdateRange(GetAll<District>());
            dbctx.Provinces.UpdateRange(GetAll<Province>());
            dbctx.Cities.UpdateRange(GetAll<City>());
            dbctx.Recipes.UpdateRange(GetAll<Recipe>());
            dbctx.Ministers.UpdateRange(GetAll<Minister>());
            dbctx.Senators.UpdateRange(GetAll<Senator>());
        }
        await dbctx.SaveChangesAsync();
    }
}