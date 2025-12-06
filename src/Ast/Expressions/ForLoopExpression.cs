using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public class ForLoopExpression : Expression
{
    public ForLoopExpression(VariableDeclaration iterator, Expression endValue, Expression loopBody)
    {
        Iterator = iterator;
        EndValue = endValue;
        LoopBody = loopBody;
    }

    public VariableDeclaration Iterator { get; }

    public Expression EndValue { get; }

    public Expression LoopBody { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}