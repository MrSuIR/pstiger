using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Expressions;

/// <summary>
/// Литеральное значение (число, строка или nil).
/// </summary>
public sealed class LiteralExpression : Expression
{
    public LiteralExpression(ValueType type, Value value)
    {
        Type = type;
        Value = value;
    }

    public ValueType Type { get; }

    public Value Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}