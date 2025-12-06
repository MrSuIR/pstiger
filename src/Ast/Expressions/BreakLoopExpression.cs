namespace PsTiger.Ast.Expressions;

public class BreakLoopExpression : Expression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}