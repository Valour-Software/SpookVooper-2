using Shared.Models.Entities;

namespace Shared.Models.Items;

public enum BuildInModifierTypes
{
    Attack = 1
}

public class Recipe
{
    public long Id {get; set; }

    public long OwnerId { get; set; }

    public BaseEntity Owner { get; set; }

    public KeyValuePair<string, int> Output { get; set; }

    public Dictionary<string, int> Inputs { get; set; }

    public string Name { get; set; }

    public double HourlyProduction { get; set; }
    public string BaseRecipeName { get; set; }
}