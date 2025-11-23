using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public class ScopeExpression : Expression
{
    public ScopeExpression(List<Declaration> declarations, Expression? expression)
    {
        Declarations = declarations;
        Expression = expression;
    }

    public List<Declaration> Declarations { get; }

    public Expression? Expression { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}