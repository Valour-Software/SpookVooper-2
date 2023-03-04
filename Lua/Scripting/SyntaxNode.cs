using System;
using SV2.Scripting;

namespace SV2.Scripting;

public enum NodeType
{
    BASE,
    ADD,
    FACTOR,
    DECIMAL,
    SYSTEMVAR,
    EXPRESSION,
    IFSTATEMENT,
    COMPARISON,
    CONDITIONALSTATEMENT,
    CONDITIONALLOGICBLOCK,
    EFFECT,
    EFFECTBODY,
    MODIFIER,
    DICTNODE,
    ADDLOCALSNODE,
    GETLOCAL
}

public class ExecutionState
{
    public Dictionary<string, decimal> Locals { get; set; }
    public District District { get; set; }
    public Province? Province { get; set; }
    public ExecutionState(District district, Province? province)
    {
        Locals = new();
        District = district;
        Province = province;
    }
}

public abstract class SyntaxNode
{
    public NodeType NodeType;
    public SyntaxNode Parent;
    public abstract decimal GetValue(ExecutionState state);
}

public abstract class ConditionalSyntaxNode : SyntaxNode
{
    public NodeType NodeType;
    public override decimal GetValue(ExecutionState state) => 0.00m;
    public abstract bool IsTrue(ExecutionState state);
}

public class Add : SyntaxNode
{
    public SyntaxNode Value;
    public Add()
    {
        NodeType = NodeType.ADD;
    }

    public override decimal GetValue(ExecutionState state)
    {
        return Value.GetValue(state);
    }
}

public class Decimal : SyntaxNode
{
    public decimal Value;
    public Decimal()
    {
        NodeType = NodeType.DECIMAL;
    }

    public override decimal GetValue(ExecutionState state)
    {
        return Value;
    }
}

public class Base : SyntaxNode
{
    public SyntaxNode Value;
    public Base()
    {
        NodeType = NodeType.BASE;
    }

    public override decimal GetValue(ExecutionState state)
    {
        return Value.GetValue(state);
    }
}


public class Factor : SyntaxNode
{
    public SyntaxNode Value;
    public Factor()
    {
        NodeType = NodeType.FACTOR;
    }
    public override decimal GetValue(ExecutionState state)
    {
        return Value.GetValue(state);
    }
}

public enum ComparisonType
{
    GREATER_THAN,
    GREATER_THAN_OR_EQUAL,
    LESS_THAN,
    LESS_THAN_OR_EQUAL,
    EQUAL
}

public class ConditionalStatementComparison : ConditionalSyntaxNode
{
    public ComparisonType comparisonType;
    public SyntaxNode LeftSide;
    public SyntaxNode RightSide;
    public ConditionalStatementComparison()
    {
        NodeType = NodeType.COMPARISON;
    }

    public override bool IsTrue(ExecutionState state)
    {
        return comparisonType switch
        {
            ComparisonType.EQUAL => LeftSide.GetValue(state) == RightSide.GetValue(state),
            ComparisonType.GREATER_THAN => LeftSide.GetValue(state) > RightSide.GetValue(state),
            ComparisonType.GREATER_THAN_OR_EQUAL => LeftSide.GetValue(state) >= RightSide.GetValue(state),
            ComparisonType.LESS_THAN => LeftSide.GetValue(state) < RightSide.GetValue(state),
            ComparisonType.LESS_THAN_OR_EQUAL => LeftSide.GetValue(state) <= RightSide.GetValue(state),
        };
    }
}

public enum ConditionalLogicBlockType
{
    AND,
    OR,
    NOT,
    NOR
}

public class ConditionalLogicBlockStatement : ConditionalSyntaxNode
{
    public List<ConditionalSyntaxNode> Children;
    public ConditionalLogicBlockType Type;
    public ConditionalLogicBlockStatement()
    {
        NodeType = NodeType.CONDITIONALLOGICBLOCK;
        Children = new();
    }

    public override bool IsTrue(ExecutionState state)
    {
        return Type switch
        {
            ConditionalLogicBlockType.AND => Children.All(x => x.IsTrue(state)),
            ConditionalLogicBlockType.OR => Children.Any(x => x.IsTrue(state)),
            ConditionalLogicBlockType.NOR => !Children.Any(x => x.IsTrue(state)),
            ConditionalLogicBlockType.NOT => !Children.All(x => x.IsTrue(state))
        };
    }
}

public class ConditionalStatement : ConditionalSyntaxNode
{
    public List<ConditionalSyntaxNode> Conditionals;
    public ConditionalStatement()
    {
        NodeType = NodeType.CONDITIONALSTATEMENT;
    }

    public override bool IsTrue(ExecutionState state)
    {
        return Conditionals.All(x => x.IsTrue(state));
    }
}

public class IfStatement : ConditionalSyntaxNode, IEffectNode
{
    public ConditionalStatement Limit;
    public ExpressionNode ValueNode;
    public EffectBody EffectNode;
    public EffectType effectType => EffectType.None;

    public IfStatement()
    {
        NodeType = NodeType.FACTOR;
    }

    public void Execute(ExecutionState state)
    {
        if (IsTrue(state))
            EffectNode.Execute(state);
    }

    public override decimal GetValue(ExecutionState state)
    {
        if (Limit.IsTrue(state))
        {
            var value = ValueNode.GetValue(state);
            if (value != 99999999999999999999999.99999m)
                return value;
        }
        return Parent.NodeType switch
        {
            NodeType.ADD => 0.00m,
            NodeType.FACTOR => 1.00m,
            NodeType.BASE => 0.00m,
            NodeType.IFSTATEMENT => 99999999999999999999999.99999m,
            _ => 99999999999999999999999.99999m
        };
    }

    public override bool IsTrue(ExecutionState state)
    {
        return true;
    }
}

public class SystemVar : SyntaxNode
{
    public string Value;
    public SystemVar()
    {
        NodeType = NodeType.SYSTEMVAR;
    }

    public static string CleanUp(string value)
    {
        return value
            .Replace("[", ".").Replace("]", "");//.Replace("\"", "");
    }

    public override decimal GetValue(ExecutionState state)
    {
        var levels = CleanUp(Value).Split(".").ToList();
        return levels[0].ToLower() switch
        {
            "district" => levels[1].ToLower() switch
            {
                "population" => state.District.TotalPopulation
            },
            "province" => levels[1].ToLower() switch
            {
                "population" => state.Province.Population,
                "owner" => state.Province.District.Id,
                "totaloftype" => state.Province.GetBuildings().Count(x => x.BuildingObj.Name.ToLower() == levels[2])
            },
            _ => 0.00m
        };
    }
}

public class ExpressionNode : SyntaxNode
{
    public List<SyntaxNode> Body { get; set; }

    public ExpressionNode()
    {
        Body = new();
        NodeType = NodeType.EXPRESSION;
    }

    public override decimal GetValue(ExecutionState state)
    {
        decimal result = 0.00m;

        foreach (var node in Body)
        {
            switch (node.NodeType)
            {
                case NodeType.BASE:
                    result = node.GetValue(state);
                    break;
                case NodeType.ADD:
                    result += node.GetValue(state);
                    break;
                case NodeType.FACTOR:
                    result *= node.GetValue(state);
                    break;
                case NodeType.GETLOCAL:
                    result = node.GetValue(state);
                    break;
                case NodeType.SYSTEMVAR:
                    result = node.GetValue(state);
                    break;
                default:
                    break;
            }
        }

        return result;
    }
}

public class DictNode : SyntaxNode
{
    public Dictionary<string, SyntaxNode> Body { get; set; }

    public Dictionary<string, decimal> PermanentValues { get; set; }

    public DictNode()
    {
        Body = new();
        PermanentValues = new();
        NodeType = NodeType.DICTNODE;
    }

    public override decimal GetValue(ExecutionState state) { return 0.00m; }

    public Dictionary<string, decimal> Evaluate(ExecutionState state)
    {
        var data = new Dictionary<string, decimal>();
        foreach ((var key, var value) in PermanentValues)
            data[key] = value;
        foreach ((var key, var valuenode) in Body) 
        {
            if (key == "add_locals") {
                foreach (var _node in ((ExpressionNode)valuenode).Body) {
                    var node = (AddLocalsNode)_node;
                    node.Execute(state);
                }
            }
                
            else
                data[key] = valuenode.GetValue(state);
        }
        return data;
    }
}

public class GetLocal : SyntaxNode
{
    public string Name;
    public GetLocal() {
        NodeType = NodeType.GETLOCAL;
    }

    public override decimal GetValue(ExecutionState state) {
        return state.Locals[Name];
    }
}

public class AddLocalsNode : SyntaxNode 
{
    public Dictionary<string, SyntaxNode> Body { get; set; }

    public AddLocalsNode() {
        Body = new();
        NodeType = NodeType.ADDLOCALSNODE;
    }

    public override decimal GetValue(ExecutionState state) { return 0.00m; }

    public void Execute(ExecutionState state)
    {
        foreach ((var key, var valuenode) in Body) 
        {
            state.Locals[key] = valuenode.GetValue(state);
        }
    }
}

public class LocalNode : SyntaxNode 
{
    public string Name { get; set; }
    public SyntaxNode Value { get; set; }
    public override decimal GetValue(ExecutionState state) { return 0.00m; }
}