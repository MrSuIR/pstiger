using PsTiger.Runtime;

namespace PsTiger.Ast.Expressions;

public sealed class LiteralExpression : Expression
{
    public LiteralExpression(Value value)
    {
        Value = value;
    }

    public Value Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}