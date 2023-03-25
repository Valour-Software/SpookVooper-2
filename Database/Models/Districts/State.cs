using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Districts;
public class State : BaseEntity {
    [Column("name", TypeName = "VARCHAR(64)")]
    public string? Name { get; set; }

    [Column("description", TypeName = "VARCHAR(512)")]
    public string? Description { get; set; }

    [Column("mapcolor")]
    public string MapColor { get; set; }

    [Column("groupid")]
    public long GroupId { get; set; }

    [NotMapped]
    public Group Group => DBCache.Get<Group>(GroupId)!;

    [Column("districtid")]
    public long DistrictId { get; set; }

    [NotMapped]
    public District District { get; set; }

    [Column("governorid")]
    public long? GovernorId { get; set; }

    [NotMapped]
    public BaseEntity? Governor => Find(GovernorId);

    [NotMapped]
    public IEnumerable<Province> Provinces => DBCache.GetAll<Province>().Where(x => x.StateId == Id);

    [NotMapped]
    public long Population => Provinces.Sum(x => x.Population);
}