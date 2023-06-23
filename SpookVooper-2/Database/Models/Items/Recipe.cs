using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using System.Text.Json.Serialization;
using SV2.Scripting.LuaObjects;
using Shared.Client;
using SV2.Scripting;

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
    public string OutputItemName { get; set; }
    public bool HasBeenUsed { get; set; }
    public DateTime Created { get; set; }

    [NotMapped]
    public BaseRecipe? BaseRecipe => GameDataManager.BaseRecipeObjs.Values.FirstOrDefault(x => x.Id == BaseRecipeId);

    [NotMapped]
    public Dictionary<ItemModifierType, double> Modifiers { get; set; }

    public bool CanUse(BaseEntity entity)
    {
        if (OwnerId == entity.Id)
            return true;
        if (EntityIdsThatCanUseThisRecipe.Contains(entity.Id))
            return true;
        return false;
    }
    public bool CanUse(long entityid)
    {
        if (OwnerId == entityid)
            return true;
        if (EntityIdsThatCanUseThisRecipe.Contains(entityid))
            return true;
        return false;
    }

    public void UpdateOutputs()
    {
        Outputs = new();
        if (BaseRecipe.OutputWithCustomItem is not null)
            Outputs[(long)CustomOutputItemDefinitionId] = BaseRecipe.OutputWithCustomItem.Value.Value;

        foreach (var pair in BaseRecipe.Outputs)
        {
            Outputs[pair.Key] = pair.Value;
        }
    }

    public void UpdateOrAddModifier(ItemModifierType type, double value)
    {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = value;
        else
            Modifiers[type] += value;
    }

    public void UpdateModifiers()
    {
        Modifiers = new();

        var value_executionstate = new ExecutionState(null, null, parentscopetype: ScriptScopeType.Recipe, recipe: this);
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var pair in EditsLevels)
        {
            var edit = BaseRecipe.LuaRecipeEdits[pair.Key];
            value_executionstate.RecipeEdit = edit;
            foreach (var modifiernode in edit.ModifierNodes)
            {
                var value = (double)modifiernode.GetValue(value_executionstate);
                UpdateOrAddModifier((ItemModifierType)modifiernode.itemModifierType!, value);
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

        var value_executionstate = new ExecutionState(null, null, parentscopetype: ScriptScopeType.Recipe, recipe: this);
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var pair in EditsLevels)
        {
            var edit = BaseRecipe.LuaRecipeEdits[pair.Key];
            value_executionstate.RecipeEdit = edit;
            foreach ((var resource, var amount) in edit.Costs.Evaluate(value_executionstate))
            {
                //Console.WriteLine(resource);
                var itemdef = DBCache.GetAll<ItemDefinition>().FirstOrDefault(x => x.Name.ToLower().Replace(" ", "_") == resource);
                if (!Inputs.ContainsKey(itemdef.Id))
                    Inputs[itemdef.Id] = 0;
                Inputs[itemdef.Id] += (double)amount;
            }
        }
    }
}