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
        foreach(var _obj in dbctx.Ministers)
            Put(_obj.UserId, _obj);
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
        if (false) {
            dbctx.Groups.UpdateRange(GetAll<Group>());
            dbctx.GroupRoles.UpdateRange(GetAll<GroupRole>());
            dbctx.Users.UpdateRange(GetAll<SVUser>());
            dbctx.TaxPolicies.UpdateRange(GetAll<TaxPolicy>());
            dbctx.SVItemOwnerships.UpdateRange(GetAll<SVItemOwnership>());
            dbctx.ItemDefinitions.UpdateRange(GetAll<ItemDefinition>());
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