using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Corporations;
using SV2.Database.Models.Factories;

namespace SV2.Database;

public class DBCacheItemAddition
{
    public Type Type { get; set; }
    public object Item { get; set; }

    public void AddToDB()
    {
        if (Type == typeof(Corporation))
            DBCache.dbctx.Add((Corporation)Item);
        else if (Type == typeof(CorporationShare))
            DBCache.dbctx.Add((CorporationShare)Item);
        else if (Type == typeof(CorporationShareClass))
            DBCache.dbctx.Add((CorporationShareClass)Item);
    }
}

public static class DBCache
{
    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static Dictionary<Type, ConcurrentDictionary<long, object>> HCache = new();

    public static ConcurrentQueue<DBCacheItemAddition> ItemQueue = new();

    public static VooperDB dbctx { get; set; }

    public static Group Vooperia => Get<Group>(100)!;

    /// <summary>
    /// ProvinceId : List<ProducingBuilding>
    /// </summary>
    public static Dictionary<long, List<ProducingBuilding>> ProvincesBuildings = new();

    public static List<ProducingBuilding> GetAllProducingBuildings() 
    {
        return ProvincesBuildings.SelectMany(x => x.Value).ToList();
    }

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

    public static void AddNew<T>(long Id, T? obj) where T : class
    {
        Put(Id, obj);
        ItemQueue.Enqueue(new() { Type = typeof(T), Item = obj });
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
            group.SVItemsOwnerships = new();
            Put(group.Id, group);
        }
        foreach(SVUser user in dbctx.Users) {
            user.SVItemsOwnerships = new();
            Put(user.Id, user);
        }
        foreach(TaxPolicy policy in dbctx.TaxPolicies) {
            Put(policy.Id, policy);
        }
        foreach(District district in dbctx.Districts) {
            Put(district.Id, district);
        }
        foreach(Province province in dbctx.Provinces) {
            province.District = Get<District>(province.DistrictId);
            ProvincesBuildings[province.Id] = new();
            Put(province.Id, province);
        }
        foreach(Factory _obj in dbctx.Factories) {
            ProvincesBuildings[_obj.ProvinceId].Add(_obj);
            Put(_obj.Id, _obj);
        }
        foreach(Farm _obj in dbctx.Farms) {
            ProvincesBuildings[_obj.ProvinceId].Add(_obj);
            Put(_obj.Id, _obj);
        }
        foreach(Mine _obj in dbctx.Mines) {
            ProvincesBuildings[_obj.ProvinceId].Add(_obj);
            Put(_obj.Id, _obj);
        }
        foreach(Infrastructure _obj in dbctx.Infrastructures) {
            ProvincesBuildings[_obj.ProvinceId].Add(_obj);
            Put(_obj.Id, _obj);
        }
        foreach(UBIPolicy policy in dbctx.UBIPolicies) {
            Put(policy.Id, policy);
        }
        foreach(GroupRole role in dbctx.GroupRoles) {
            Put(role.Id, role);
        }
        foreach(Election election in dbctx.Elections) {
            Put(election.Id, election);
        }
        foreach(var _obj in dbctx.Votes)
            Put(_obj.Id, _obj);
        foreach (var _obj in dbctx.States)
            Put(_obj.Id, _obj);
        foreach (var _obj in dbctx.Recipes)
            Put(_obj.Id, _obj);
        foreach (var _obj in dbctx.Senators)
            Put(_obj.DistrictId, _obj);
        foreach (var _obj in dbctx.Corporations)
            Put(_obj.Id, _obj);
        foreach (var _obj in dbctx.CorporationShareClasses)
            Put(_obj.Id, _obj);

        foreach (District district in GetAll<District>())
        {
            district.Provinces = GetAll<Province>().Where(x => x.DistrictId == district.Id).ToList();
        }

        foreach (SVItemOwnership item in dbctx.SVItemOwnerships) 
        {
            var entity = FindEntity(item.OwnerId);
            entity.SVItemsOwnerships[item.DefinitionId] = item;
            Put(item.Id, item);
        }
        foreach (ItemDefinition definition in dbctx.ItemDefinitions) {
            Put(definition.Id, definition);
        }

        foreach (var _obj in dbctx.States)
            Put(_obj.Id, _obj);

        //#endif
    }

    public static async Task SaveAsync()
    {
        while (ItemQueue.Count > 0)
        {
            if (ItemQueue.TryDequeue(out var item))
                item.AddToDB();
        }
        await dbctx.SaveChangesAsync();
    }
}