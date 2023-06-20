using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Shared.Managers;

namespace Shared.Models.Items;

public enum ItemModifierType
{
    Attack = 1
}

public class Recipe
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }
    public string StringId { get; set; }
    public long OwnerId { get; set; }

    //public BaseEntity Owner => BaseEntity.Find(OwnerId)!;

    /// <summary>
    /// DefId : amount
    /// </summary>
    public Dictionary<long, double> Outputs { get; set; }

    /// <summary>
    /// DefId : amount
    /// </summary>
    public Dictionary<long, double> Inputs { get; set; }
    public double PerHour { get; set; }
    public string? BaseRecipeId { get; set; }
    public bool Obsolete { get; set; }
    public List<long> EntityIdsThatCanUseThisRecipe { get; set; }
    public Dictionary<string, int> EditsLevels { get; set; }
    public Dictionary<string, long> AnyWithBaseTypesFilledIn { get; set; }
    public Dictionary<string, long> CustomOutputItemDefinitionsIds { get; set; }

    public async ValueTask<BaseRecipe> GetBaseRecipeAsync()
    {
        return await BaseRecipe.FindAsync(BaseRecipeId);
    }

    public bool CanUse(BaseEntity entity)
    {
        if (OwnerId == entity.Id)
            return true;
        if (EntityIdsThatCanUseThisRecipe.Contains(entity.Id))
            return true;
        return false;
    }

    public async ValueTask UpdateOutputs()
    {
        Outputs = new();
        foreach (var pair in (await GetBaseRecipeAsync()).Outputs)
        {
            Outputs[pair.Key] = pair.Value;
        }
    }

    public async ValueTask UpdateInputs()
    {
        Inputs = new();
        foreach (var pair in (await GetBaseRecipeAsync()).Inputs)
        {
            Inputs[pair.Key] = pair.Value;
        }
    }
}