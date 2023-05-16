using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Government;

public class Senator
{
    [Key]
    public long DistrictId { get; set; }

    public District District => DBCache.Get<District>(DistrictId)!;

    public long UserId { get; set; }

    public SVUser User => DBCache.Get<SVUser>(UserId)!;
}
