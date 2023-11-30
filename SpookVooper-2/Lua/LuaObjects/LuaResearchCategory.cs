namespace SV2.Scripting.LuaObjects;

public abstract class LuaResearchHasChildren
{
    public List<LuaResearchPrototype> Children { get; set; }
}

public class LuaResearchCategory : LuaResearchHasChildren
{
    public string Id { get; set; }
    public string Name { get; set; }
}
