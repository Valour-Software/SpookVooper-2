using SV2.Scripting.LuaObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Districts;

public class BuildingRequest
{
    [Key]
    public long Id { get; set; }
    public long RequesterId { get; set; }

    [NotMapped]
    public BaseEntity Requester => BaseEntity.Find(RequesterId);
    public long ProvinceId { get; set; }

    [NotMapped]
    public Province Province => DBCache.Get<Province>(ProvinceId)!;

    /// <summary>
    /// These should be null for a new building but should NOT be
    /// if we are simply adding levels to an existing building
    /// </summary>
    public long? BuildingId { get; set; }

    [NotMapped]
    public BuildingBase? Building => BuildingBase.Find(BuildingId);

    public string BuildingObjId { get; set; }

    [NotMapped]
    public LuaBuilding BuildingLuaObj => GameDataManager.BaseBuildingObjs[BuildingObjId];

    public int LevelsRequested { get; set; }
    public DateTime Applied { get; set; }

    /// <summary>
    /// The time that this request was granted or denied
    /// </summary>
    public DateTime? ActionTime { get; set; }
    public bool Reviewed { get; set; }

    /// <summary>
    /// True if this request was granted, false if not
    /// </summary>
    public bool Granted { get; set; }

    /// <summary>
    /// The id of the entity who reviewed this request
    /// </summary>
    public long? ReviewerId { get; set; }
}
