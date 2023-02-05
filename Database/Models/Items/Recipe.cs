using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;

namespace SV2.Database.Models.Items;

public enum BuildInModifierTypes
{
    Attack = 1
}

public class Recipe : IHasOwner
{
    [Key]
    public long Id {get; set; }

    public long OwnerId { get; set; }

    [NotMapped]
    public BaseEntity Owner { 
        get {
            return BaseEntity.Find(OwnerId)!;
        }
    }

    [Column(TypeName = "jsonb")]
    public KeyValuePair<string, int> Output { get; set; }

    [Column(TypeName = "jsonb")]
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