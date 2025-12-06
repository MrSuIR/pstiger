using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public class ForLoopExpression : Expression
{
    public ForLoopExpression(string iteratorName, Expression startValue, Expression endValue, Expression loopBody)
    {
        Iterator = new ForIteratorDeclaration(iteratorName);
        StartValue = startValue;
        EndValue = endValue;
        LoopBody = loopBody;
    }

    public ForIteratorDeclaration Iterator { get; }

    public Expression StartValue { get; }

    public Expression EndValue { get; }

    public Expression LoopBody { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}