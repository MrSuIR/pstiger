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

        foreach (Expression nested in e.Expressions)
        {
            nested.Accept(this);
        }
    }

    public virtual void Visit(VariableAccessExpression e)
    {
    }

    public virtual void Visit(AssignmentExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
    }

    public virtual void Visit(IfElseExpression e)
    {
        e.Condition.Accept(this);
        e.ThenBranch.Accept(this);
        e.ElseBranch?.Accept(this);
    }

    public virtual void Visit(WhileLoopExpression e)
    {
        e.Condition.Accept(this);
        e.LoopBody.Accept(this);
    }

    public virtual void Visit(ForLoopExpression e)
    {
        e.Iterator.Accept(this);
        e.StartValue.Accept(this);
        e.EndValue.Accept(this);
        e.LoopBody.Accept(this);
    }

    public virtual void Visit(ForIteratorDeclaration d)
    {
    }

    public virtual void Visit(BreakLoopExpression e)
    {
    }

    public virtual void Visit(VariableDeclaration d)
    {
        d.InitialValue.Accept(this);
    }

    public virtual void Visit(FunctionDeclaration d)
    {
        foreach (AbstractParameterDeclaration declaration in d.Parameters)
        {
            declaration.Accept(this);
        }

        d.Body.Accept(this);
    }

    public virtual void Visit(ParameterDeclaration d)
    {
    }

    public virtual void Visit(TypeDeclaration d)
    {
        d.TypeExpression.Accept(this);
    }

    public virtual void Visit(NamedTypeExpression e)
    {
    }

    public virtual void Visit(ArrayTypeExpression e)
    {
    }
}