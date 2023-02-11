namespace SV2.Helpers;

public class MinisterHelper
{
    public static string ToReadableName(MinisterType type)
    {
        return type switch
        {
            MinisterType.ChiefFinancierofVooperia => "Chief Financier of Vooperia",
            MinisterType.ImperialElectionMinister => "Imperial Election Minister",
            MinisterType.MinisterofJournalism => "Minister of Journalism"
        };
    }
}
