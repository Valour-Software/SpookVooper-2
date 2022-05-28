using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Districts;

public enum TerrainType
{
    Plains = 1,
    Mountain = 2,
    Hills = 3,
    Urban = 4,
    Forests = 5,
    River = 6,
    Marsh = 7
}

public class Province
{
    [Key]
    [GuidID]
    public string Id { get; set;}

    [VarChar(64)]
    public string? Name { get; set;}

    [EntityId]
    public string DistrictId { get; set; }

    [ForeignKey("DistrictId")]
    public District Owner { get; set; }

    public IEnumerable<IBuilding> GetBuildings()
    {
        List<IBuilding> buildings = new();
        buildings.AddRange(DBCache.GetAll<Factory>().Where(x => x.ProvinceId == Id));
        buildings.AddRange(DBCache.GetAll<Mine>().Where(x => x.ProvinceId == Id));
        return buildings;
    }
}