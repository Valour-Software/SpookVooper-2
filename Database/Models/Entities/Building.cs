
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Buildings;

public enum BuildingType
{
    Factory = 1,
    Mine = 2
}

public interface IBuilding
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long OwnerId { get; set; }

    public BaseEntity Owner { 
        get {
            return BaseEntity.Find(OwnerId)!;
        }
    }

    public long ProvinceId { get; set; } 

    [NotMapped]
    public Province Province {
        get {
            return DBCache.Get<Province>(ProvinceId)!;
        }
    }

    BuildingType buildingType { get;}

    public string GetProduction();
}