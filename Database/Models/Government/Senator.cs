using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Government;

public class Senator
{
    [Key]
    public long DistrictId { get; set; }

    [ForeignKey(nameof(DistrictId))]
    public District District { get; set; }

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public SVUser User { get; set; }
}
