using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Expressions;

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