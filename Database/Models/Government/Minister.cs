namespace SV2.Database.Models.Government;

public enum MinisterType
{
    ImperialElectionMinister = 1,
    ChiefFinancierofVooperia = 2
}

public class Minister
{
    [GuidID]
    public string UserId { get; set; }

    public MinisterType Type { get; set; }
}