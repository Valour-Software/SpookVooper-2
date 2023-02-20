using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV2.Scripting;

public class SyntaxModifierNode : SyntaxNode
{
    public DistrictModifierType? DistrictModifierType { get; set; }
    public ProvinceModifierType? ProvinceModifierType { get; set; }
    public SyntaxNode Value { get; set; }

    public SyntaxModifierNode()
    {
        NodeType = NodeType.MODIFIER;
    }

    public override decimal GetValue(ExecutionState state)
    {
        return Value.GetValue(state);
    }

    public decimal GetValue(ExecutionState state, decimal scaleby)
    {
        return GetValue(state) * scaleby;
    }
}
