namespace PsTiger.Ast.Expressions;

public class UnaryMinusExpression : Expression
{
    public UnaryMinusExpression(Expression operand)
    {
        Operand = operand;
    }

    public Expression Operand { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}