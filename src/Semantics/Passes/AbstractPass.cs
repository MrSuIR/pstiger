using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;

namespace PsTiger.Semantics.Passes;

/// <summary>
/// Базовый класс для проходов по AST с целью вычисления атрибутов и семантических проверок.
/// </summary>
public abstract class AbstractPass : IAstVisitor
{
    public virtual void Visit(LiteralExpression e)
    {
    }

    public virtual void Visit(BinaryOperationExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
    }

    public virtual void Visit(SequenceExpression e)
    {
        foreach (Expression nested in e.Sequence)
        {
            nested.Accept(this);
        }
    }

    public virtual void Visit(UnaryMinusExpression e)
    {
        e.Operand.Accept(this);
    }

    public virtual void Visit(FunctionCallExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }
    }

    public virtual void Visit(ScopeExpression e)
    {
        foreach (Declaration declaration in e.Declarations)
        {
            declaration.Accept(this);
        }

        e.Expression?.Accept(this);
    }

    public virtual void Visit(VariableDeclaration e)
    {
        e.InitialValue.Accept(this);
    }
}