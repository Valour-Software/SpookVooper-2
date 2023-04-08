using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Corporations;

public class Corporation
{
    [Key]
    public long Id { get; set; }

    public long GroupId { get; set; }

    [NotMapped]
    public Group Group => DBCache.Get<Group>(GroupId)!;
}
