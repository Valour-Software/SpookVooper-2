using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Stats;

public enum StatType
{
    GDP,
    Population,
    TotalBuildingSlots,
    UsedBuildingSlots,
    Xp
}

public enum TargetType
{
    Global,
    District,
    State,
    User
}

[Index(nameof(Date))]
[Index(nameof(StatType))]
public class Stat
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("stattype")]
    public StatType StatType { get; set; }

    [Column("targetid")]
    public long? TargetId { get; set; }

    [Column("targettype")]
    public TargetType TargetType { get; set; }

    [Column("value")]
    public long Value { get; set; }
}
