using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Corporations;

[Index(nameof(EntityId))]
[Index(nameof(CorporationId))]
[Index(nameof(ShareClassId))]
public class CorporationShare
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("entityid")]
    public long EntityId { get; set; }

    [Column("corporationid")]
    public long CorporationId { get; set; }

    [Column("shareclassid")]
    public long ShareClassId { get; set; }

    [Column("amount")]
    public long Amount { get; set; }

    [NotMapped]
    public BaseEntity Entity => BaseEntity.Find(EntityId);

    [NotMapped]
    public Corporation Corporation => DBCache.Get<Corporation>(CorporationId)!;

    [NotMapped]
    public CorporationShareClass ShareClass => DBCache.Get<CorporationShareClass>(ShareClassId)!;
}