using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using SV2.Web;

namespace SV2.Database.Models.Items;

public class TradeItem : IHasOwner
{
    [Key]
    public long Id {get; set; }

    public long OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }
    
    public long Definition_Id { get; set; }
    
    [NotMapped]
    public TradeItemDefinition Definition { 
        get {
            return DBCache.Get<TradeItemDefinition>(Definition_Id)!;
        }
    }
    public int Amount { get; set;}
}

public class BuiltinModifier
{
    public int Level { get; set; }
    
    public BuildInModifierTypes ModifierType { get; set; }

    public string RecipeName { get; set; }
    
    [NotMapped]
    public ModifierLevelDefinition ModifierLevelDefinition {
        get {
            return ResourceManager.ModifierLevelDefinitions.FirstOrDefault(x => x.RecipeName == RecipeName && x.ModifierType == ModifierType)!;
        }
    }
}

public class TradeItemDefinition : IHasOwner
{
    [Key]
    public long Id {get; set; }

    public long OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }
    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(1024)]
    public string? Description { get; set; }
    public DateTime Created { get; set; }

    [Column(TypeName = "jsonb")]
    public List<BuiltinModifier> BuiltinModifiers { get; set; }

    // json list of modifiers
    public string? Modifiers { get; set; }

    public TradeItemDefinition()
    {

    }

    public TradeItemDefinition(long ownerid, string name)
    {
        Id = IdManagers.TradeItemDefinitionIdGenerator.Generate();
        OwnerId = ownerid;
        Name = name;
        Created = DateTime.UtcNow;
    }
}