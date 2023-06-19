using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using System.Text.Json.Serialization;

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
    [JsonIgnore]
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    [Column(TypeName = "jsonb")]
    public Dictionary<string, int> Output { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, int> Inputs { get; set; }

    public string Name { get; set; }
    public double PerHour { get; set; }
    public string? BaseRecipeId { get; set; }

    public BaseRecipe? BaseRecipe => GameDataManager.BaseRecipeObjs.Values.FirstOrDefault(x => x.Id == BaseRecipeId);
}