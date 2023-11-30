using System.ComponentModel.DataAnnotations;

namespace SV2.Database.Models.Entities;

public class JobApplication
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }
    public long BuildingId { get; set; }
    public bool Reviewed { get; set; }
    public bool Accepted { get; set; }
}
