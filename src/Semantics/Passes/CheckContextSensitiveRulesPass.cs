using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Semantics.Exceptions;

namespace PsTiger.Semantics.Passes;

/// <summary>
/// Проверяет соблюдение контекстно-зависимых правил языка.
/// </summary>
/// <remarks>
/// Контекстно-зависимые правила не могли быть проверены при синтаксическом анализе, поскольку синтаксический анализатор
///  разбирает контекстно-свободную грамматику.
/// </remarks>
public sealed class CheckContextSensitiveRulesPass : AbstractPass
{
    private readonly Stack<ExpressionContext> _expressionContextStack;

    public CheckContextSensitiveRulesPass()
    {
        _expressionContextStack = [];
        _expressionContextStack.Push(ExpressionContext.None);
    }

    private enum ExpressionContext
    {
        None,
        InsideLoop,
    }

    /// <summary>
    /// Проверяет корректность программы с точки зрения использования функций.
    /// </summary>
    /// <exception cref="InvalidFunctionCallException">Бросается при неправильном вызове функций.</exception>
    public override void Visit(FunctionCallExpression e)
    {
        base.Visit(e);

        if (e.Arguments.Count != e.Function.Parameters.Count)
        {
            throw new InvalidFunctionCallException(
                $"Function {e.Name} requires {e.Function.Parameters.Count} arguments, got {e.Arguments.Count}"
            );
        }
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);

        if (e.Left is VariableAccessExpression variableAccessExpression)
        {
            if (variableAccessExpression.Variable is ForIteratorDeclaration)
            {
                throw new InvalidAssignmentException("Assigning a for loop iterator is not allowed");
            }
        }
        else
        {
            throw new InvalidAssignmentException("Left side of assignment must be a variable access expression");
        }
    }

    public override void Visit(WhileLoopExpression e)
    {
        _expressionContextStack.Push(ExpressionContext.InsideLoop);
        try
        {
            base.Visit(e);
        }
        finally
        {
            _expressionContextStack.Pop();
        }
    }

    public override void Visit(ForLoopExpression e)
    {
        _expressionContextStack.Push(ExpressionContext.InsideLoop);
        try
        {
            base.Visit(e);
        }
        finally
        {
            _expressionContextStack.Pop();
        }
    }

    public override void Visit(BreakLoopExpression e)
    {
        base.Visit(e);

        if (_expressionContextStack.Peek() != ExpressionContext.InsideLoop)
        {
            throw new InvalidExpressionException("The \"break\" expression is allowed only inside the loop");
        }
    }
}