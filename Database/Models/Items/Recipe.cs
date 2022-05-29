using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;

namespace SV2.Database.Models.Items;

public enum BuildInModifierTypes
{
    Attack = 1
}

public class Modifier
{
    public double Value { get; set; }
    public BuildInModifierTypes ModifierType { get; set; }
}

public class Recipe : IHasOwner
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    public KeyValuePair<string, int> Output { get; set; }
    public Dictionary<string, int> Inputs { get; set; }

    public string Name { get; set; }

    public double HourlyProduction { get; set; }
    public string BaseRecipeName { get; set; }

    public BaseRecipe baseRecipe {
        get {
            return ResourceManager.Recipes.FirstOrDefault(x => x.Name == BaseRecipeName)!;
        }
    }
}