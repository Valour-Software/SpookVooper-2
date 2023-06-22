using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using System.Text.Json.Serialization;
using SV2.Scripting.LuaObjects;

namespace SV2.Database.Models.Items;

public enum ItemModifierType
{
    Attack = 1
}

public class Recipe : IHasOwner
{
    [Key]
    public long Id {get; set; }
    public string Name { get; set; }
    public string StringId { get; set; }
    public long OwnerId { get; set; }
    public long? CustomOutputItemDefinitionId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    /// <summary>
    /// DefId : amount
    /// </summary>
    [Column(TypeName = "jsonb")]
    public Dictionary<long, double> Outputs { get; set; }

    /// <summary>
    /// DefId : amount
    /// </summary>
    [Column(TypeName = "jsonb")]
    public Dictionary<long, double> Inputs { get; set; }
    public double PerHour { get; set; }
    public string? BaseRecipeId { get; set; }
    public bool Obsolete { get; set; }

    [Column(TypeName = "bigint[]")]
    public List<long> EntityIdsThatCanUseThisRecipe { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, int> EditsLevels { get; set; }

    /// <summary>
    /// AnyWith.Id : item defid
    /// </summary>
    [Column(TypeName = "jsonb")]
    public Dictionary<string, long> AnyWithBaseTypesFilledIn { get; set; }

    public bool HasBeenUsed { get; set; }

    [NotMapped]
    public BaseRecipe? BaseRecipe => GameDataManager.BaseRecipeObjs.Values.FirstOrDefault(x => x.Id == BaseRecipeId);

    public bool CanUse(BaseEntity entity)
    {
        if (OwnerId == entity.Id)
            return true;
        if (EntityIdsThatCanUseThisRecipe.Contains(entity.Id))
            return true;
        return false;
    }

    public void UpdateOutputs()
    {
        Outputs = new();
        foreach (var pair in BaseRecipe.Outputs)
        {
            if (pair.Key == 0)
            {
                Outputs[(long)CustomOutputItemDefinitionId] = pair.Value;
            }
            else {
                Outputs[pair.Key] = pair.Value;
            }
        }
    }

    public void UpdateInputs()
    {
        Inputs = new();
        foreach (var pair in BaseRecipe.Inputs)
        {
            Inputs[pair.Key] = pair.Value;
        }
        foreach (var anywith in BaseRecipe.AnyWithBaseTypes)
        {
            if (!AnyWithBaseTypesFilledIn.ContainsKey(anywith.Id))
                continue;
            Inputs[AnyWithBaseTypesFilledIn[anywith.Id]] = anywith.Amount;
        }
    }
}