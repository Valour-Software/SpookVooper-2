using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SV2.Scripting.Parser;
using SV2.Scripting;

namespace SV2.Managers;

public class BaseRecipe
{
    public Dictionary<string, double> Inputs { get; set; }
    public Dictionary<string, double> Outputs { get; set; }
    public string Id { get; set; }
    public double PerHour { get; set; }
    public bool Editable { get; set; }
    public bool Inputcost_Scaleperlevel { get; set; }

    public BaseRecipe() {
        Inputs = new Dictionary<string, double>();
        Outputs = new Dictionary<string, double>();
    }
    public string Name { get; set; }

    public List<SyntaxModifierNode>? ModifierNodes { get; set; }
}

public class ConsumerGood
{
    public string Name { get; set; }

    // in x per 1,000 citizens per year
    public double PopGrowthRateModifier { get; set; }

    // the score per 10k citizens that have this good filled
    public double EconomicScoreModifier { get; set; }
    
    // 10k citizens will consume this many units per hour
    public double PopConsumptionRate { get; set; }
}

public class SVResource 
{
    public string Name { get; set; }
    public ConsumerGood? consumerGood { get; set; }

    public ItemDefinition ItemDefinition { get; set; }
}

public static class ResourceManager 
{
    public static List<string> GetFilePaths(string path)
    {
        if (path.Contains("/"))
        {
            return Directory.GetFiles(path).ToList();
        }
        try
        {
            return Directory.GetFiles($"../../../Managers/Data/{path}").ToList();
        }
        catch
        {
            return Directory.GetFiles($"Managers/Data/{path}").ToList();
        }
    }
}