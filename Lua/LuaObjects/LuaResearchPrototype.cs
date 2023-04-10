namespace SV2.Scripting.LuaObjects;

public enum WhoCanResearch
{
    District,
    NonDistrict
}

public class LuaResearchPrototype : LuaResearchHasChildren
{
    public string Id { get; set; }
    public string Name { get; set; }
    public WhoCanResearch WhoCanResearch { get; set; }
    public DictNode Costs { get; set; }
    public string Color { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
    public bool IsInfinite { get; set; }
    public string CategoryId { get; set; }
}

public class LuaResearch
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Depth { get; set; }
    public int Level { get; set; }
    public string LuaResearchPrototypeId { get; set; }
    public LuaResearchPrototype LuaResearchPrototype { get; set; }
    public string ParentId { get; set; }

    public List<LuaResearch> Children { get; set; }
}