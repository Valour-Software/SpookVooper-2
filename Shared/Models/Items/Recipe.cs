using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Shared.Managers;
using Shared.Models.Districts;

namespace Shared.Models.Items;

public enum ItemModifierType
{
    Attack = 1,
    AttackFactor = 2
}

public class Recipe : Item
{
    [NotMapped]
    [JsonIgnore]
    public override string BaseRoute => $"api/recipes";
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
    public long? CustomOutputItemDefinitionId { get; set; }
    public Dictionary<ItemModifierType, double> Modifiers { get; set; }
    public string OutputItemName { get; set; }
    public bool HasBeenUsed { get; set; }
    public async ValueTask<BaseRecipe> GetBaseRecipeAsync()
    {
        return await BaseRecipe.FindAsync(BaseRecipeId);
    }

    public static async ValueTask<Recipe> FindAsync(string id, bool refresh = false)
    {
        if (!refresh)
        {
            var cached = SVCache.Get<Recipe>(id);
            if (cached is not null)
                return cached;
        }

        var item = (await SVClient.GetJsonAsync<Recipe>($"api/recipes/{id}")).Data;

        if (item is not null)
            await item.AddToCache();

        return item;
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

    public void UpdateOrAddModifier(ItemModifierType type, double value)
    {
        if (!Modifiers.ContainsKey(type))
            Modifiers[type] = value;
        else
            Modifiers[type] += value;
    }

    public async ValueTask UpdateModifiers()
    {
        Modifiers = new();

        var baserecipe = await GetBaseRecipeAsync();
        var value_executionstate = new ExecutionState(null, null, parentscopetype: ScriptScopeType.Recipe, recipe: this);
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var pair in EditsLevels)
        {
            var edit = baserecipe.LuaRecipeEdits[pair.Key];
            value_executionstate.RecipeEdit = edit;
            foreach (var modifiernode in edit.ModifierNodes)
            {
                var value = (double)modifiernode.GetValue(value_executionstate);
                UpdateOrAddModifier((ItemModifierType)modifiernode.itemModifierType!, value);
            }
        }
    }

    public async ValueTask UpdateInputs()
    {
        Inputs = new();
        foreach (var pair in (await GetBaseRecipeAsync()).Inputs)
        {
            Inputs[pair.Key] = pair.Value;
        }

        var baserecipe = await GetBaseRecipeAsync();
        var value_executionstate = new ExecutionState(null, null, parentscopetype: ScriptScopeType.Recipe, recipe: this);
        //var scaleby_executionstate = new ExecutionState(District, this);
        foreach (var pair in EditsLevels)
        {
            var edit = baserecipe.LuaRecipeEdits[pair.Key];
            value_executionstate.RecipeEdit = edit;
            foreach ((var resource, var amount) in edit.Costs.Evaluate(value_executionstate))
            {
                //Console.WriteLine(resource);
                var itemdef = SVCache.GetAll<ItemDefinition>().FirstOrDefault(x => x.Name.ToLower().Replace(" ", "_") == resource);
                if (!Inputs.ContainsKey(itemdef.Id))
                    Inputs[itemdef.Id] = 0;
                Inputs[itemdef.Id] += (double)amount;
            }
        }
    }

    public async ValueTask<string> GetBaseOutput()
    {
        return (await GetBaseRecipeAsync()).OutputWithCustomItem.Value.Key;
    }

    public override async Task AddToCache()
    {
        await SVCache.Put(Id, this);
    }
}