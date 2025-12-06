namespace PsTiger.Ast.Expressions;

public class WhileLoopExpression : Expression
{
    public WhileLoopExpression(Expression condition, Expression loopBody)
    {
        Condition = condition;
        LoopBody = loopBody;
    }

    public Expression Condition { get; }

    public Expression LoopBody { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}