using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Corporations;

public enum ShareClassName
{
    A = 0,
    B = 1,
    C = 2,
    D = 3,
    E = 4
}

public enum ShareClassType
{
    Preferred,
    Common
}

public class CorporationShareClass
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("corporationid")]
    public long CorporationId { get; set; }

    /// <summary>
    /// Class A, Class B, etc
    /// </summary>
    [Column("classname")]
    public ShareClassName ClassName { get; set; }

    /// <summary>
    /// Preferred or common
    /// </summary>
    [Column("classtype")]
    public ShareClassType ClassType { get; set; }

    [Column("votingpower", TypeName = "numeric(9, 3)")]
    public decimal VotingPower { get; set; }

    /// <summary>
    /// Dividend payment per share per month
    /// </summary>
    [Column("dividendrate", TypeName = "numeric(9, 3)")]
    public decimal DividendRate { get; set; }

    [NotMapped]
    public Corporation Corporation => DBCache.Get<Corporation>(CorporationId)!;
}
