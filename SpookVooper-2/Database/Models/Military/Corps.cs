using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Military;

public class Corps
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// The Group that owns this corps
    /// </summary>
    public long OwnerId { get; set; }
    public List<long> CommandersIds { get; set; }
    public long FieldArmyId { get; set; }
}
