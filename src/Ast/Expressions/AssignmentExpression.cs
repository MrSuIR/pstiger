namespace PsTiger.Ast.Expressions;

/// <summary>
/// Выражение присваивания.
/// </summary>
public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(VariableAccessExpression left, Expression right)
    {
        Left = left;
        Right = right;
    }

    public VariableAccessExpression Left { get; }

    public Expression Right { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}