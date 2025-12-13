namespace PsTiger.Ast.Expressions;

public sealed class ArrayAccessExpression : Expression
{
    public ArrayAccessExpression(Expression array, Expression index)
    {
        Array = array;
        Index = index;
    }

    public Expression Array { get; }

    public Expression Index { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}