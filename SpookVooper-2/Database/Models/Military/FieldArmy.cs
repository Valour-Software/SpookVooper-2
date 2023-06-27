using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Military;

public class FieldArmy
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// The Group that owns this field army
    /// </summary>
    public long OwnerId { get; set; }
    public List<long> CommandersIds { get; set; }
    public long ArmyGroupId { get; set; }
}
