using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV2.Scripting;

public class SyntaxModifierNode : SyntaxNode
{
    public DistrictModifierType ModifierType { get; set; }
    public SyntaxNode Value { get; set; }

    public SyntaxModifierNode()
    {
        NodeType = NodeType.MODIFIER;
    }

    public override decimal GetValue(District district)
    {
        return Value.GetValue(district);
    }

    public decimal GetValue(District district, decimal scaleby)
    {
        return GetValue(district) * scaleby;
    }
}
