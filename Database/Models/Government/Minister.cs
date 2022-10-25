using System.ComponentModel.DataAnnotations;

namespace SV2.Database.Models.Government;

public enum MinisterType
{
    ImperialElectionMinister = 1,
    ChiefFinancierofVooperia = 2
}

public class Minister
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }

    public MinisterType Type { get; set; }
}