using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Corporations;

[Index(nameof(EntityId))]
[Index(nameof(CorporationId))]
public class CorporationShare
{
    [Key]
    public long Id { get; set; }
    
    public long EntityId { get; set; }
    
    public long CorporationId { get; set; }

    public long ShareClassId { get; set; }

    [NotMapped]
    public BaseEntity Entity => BaseEntity.Find(EntityId);

    [NotMapped]
    public Corporation Corporation => DBCache.Get<Corporation>(CorporationId)!;


}
