using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public class ScopeExpression : Expression
{
    public ScopeExpression(List<Declaration> declarations, List<Expression> expressions)
    {
        Declarations = declarations;
        Expressions = expressions;
    }

    public List<Declaration> Declarations { get; }

    public List<Expression> Expressions { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}