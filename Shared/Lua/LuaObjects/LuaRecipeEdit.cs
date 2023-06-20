using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Lua.LuaObjects;

public class LuaRecipeEdit
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
    public DictNode Costs { get; set; }
}

