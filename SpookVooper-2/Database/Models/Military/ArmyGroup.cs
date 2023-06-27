using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Military;

public class ArmyGroup
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// The Group that owns this army group
    /// </summary>
    public long OwnerId { get; set; }
    public List<long> CommandersIds { get; set; }

    public List<FieldArmy> FieldArmies { get; set; }
}
