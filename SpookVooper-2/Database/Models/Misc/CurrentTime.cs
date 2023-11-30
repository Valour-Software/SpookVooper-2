using System.ComponentModel.DataAnnotations;

namespace SV2.Database.Models.Misc;

public class CurrentTime
{
    [Key]
    public long Id { get; set; } = 100;

    public DateTime Time { get; set; }
}
