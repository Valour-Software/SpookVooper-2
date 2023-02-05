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
	MODIFIER
}

public abstract class SyntaxNode
{
	public NodeType NodeType;
	public SyntaxNode Parent;
	public abstract decimal GetValue(District district);
}

public abstract class ConditionalSyntaxNode : SyntaxNode
{
	public NodeType NodeType;
	public override decimal GetValue(District district) => 0.00m;
	public abstract bool IsTrue(District district);
}

public class Add : SyntaxNode
{
	public SyntaxNode Value;
	public Add()
	{
		NodeType = NodeType.ADD;
	}

	public override decimal GetValue(District district)
	{
		return Value.GetValue(district);
	}
}

public class Decimal : SyntaxNode
{
	public decimal Value;
	public Decimal()
	{
		NodeType = NodeType.DECIMAL;
	}

	public override decimal GetValue(District district)
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

	public override decimal GetValue(District district)
	{
		return Value.GetValue(district);
	}
}


public class Factor : SyntaxNode
{
	public SyntaxNode Value;
	public Factor()
	{
		NodeType = NodeType.FACTOR;
	}
	public override decimal GetValue(District district)
	{
		return Value.GetValue(district);
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

	public override bool IsTrue(District district)
	{
		return comparisonType switch
		{
			ComparisonType.EQUAL => LeftSide.GetValue(district) == RightSide.GetValue(district),
			ComparisonType.GREATER_THAN => LeftSide.GetValue(district) > RightSide.GetValue(district),
			ComparisonType.GREATER_THAN_OR_EQUAL => LeftSide.GetValue(district) >= RightSide.GetValue(district),
			ComparisonType.LESS_THAN => LeftSide.GetValue(district) < RightSide.GetValue(district),
			ComparisonType.LESS_THAN_OR_EQUAL => LeftSide.GetValue(district) <= RightSide.GetValue(district),
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

	public override bool IsTrue(District district)
	{
		return Type switch
		{
			ConditionalLogicBlockType.AND => Children.All(x => x.IsTrue(district)),
			ConditionalLogicBlockType.OR => Children.Any(x => x.IsTrue(district)),
			ConditionalLogicBlockType.NOR => !Children.Any(x => x.IsTrue(district)),
			ConditionalLogicBlockType.NOT => !Children.All(x => x.IsTrue(district))
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

	public override bool IsTrue(District district)
	{
		return Conditionals.All(x => x.IsTrue(district));
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

	public void Execute(District district)
	{
		if (IsTrue(district))
			EffectNode.Execute(district);
	}

	public override decimal GetValue(District district)
	{
		if (Limit.IsTrue(district))
		{
			var value = ValueNode.GetValue(district);
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

	public override bool IsTrue(District district)
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

	public override decimal GetValue(District district)
	{
		var levels = CleanUp(Value).Split(".").ToList();
		return levels[0].ToLower() switch
		{
			"nation" => levels[1].ToLower() switch {
				"population" => district.TotalPopulation
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

	public override decimal GetValue(District district)
	{
		decimal result = 0.00m;

		foreach(var node in Body)
		{
			switch (node.NodeType)
			{
				case NodeType.BASE:
					result = node.GetValue(district);
					break;
				case NodeType.ADD:
					result += node.GetValue(district);
					break;
				case NodeType.FACTOR:
					result *= node.GetValue(district);
					break;
				case NodeType.SYSTEMVAR:
					break;
				default:
					break;
			}
		}

		return result;
	}
}