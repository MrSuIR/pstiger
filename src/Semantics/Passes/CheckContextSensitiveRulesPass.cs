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
    // Стек контекстов выражений используется для проверки контекстно-зависимых правил.
    private readonly Stack<ExpressionContext> _expressionContextStack;

    public CheckContextSensitiveRulesPass()
    {
        _expressionContextStack = [];
        _expressionContextStack.Push(ExpressionContext.Default);
    }

    private enum ExpressionContext
    {
        Default,
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

        // Проверяем контекстно-зависимые правила присваивания:
        // 1) Левая часть присваивания должна быть lvalue.
        // 2) Не допускается присваивание значения итератору цикла for.
        if (!IsLvalue(e.Left))
        {
            throw new InvalidAssignmentException("Left side of assignment must be a lvalue");
        }

        if (e.Left is VariableAccessExpression { Variable: ForIteratorDeclaration })
        {
            throw new InvalidAssignmentException("Assigning a for loop iterator is not allowed");
        }
    }

    public override void Visit(FunctionDeclaration d)
    {
        // Меняем текущий контекст: дочерние узлы AST находятся в контексте по умолчанию.
        _expressionContextStack.Push(ExpressionContext.Default);
        try
        {
            base.Visit(d);
        }
        finally
        {
            _expressionContextStack.Pop();
        }
    }

    public override void Visit(WhileLoopExpression e)
    {
        // Меняем текущий контекст: дочерние узлы AST находятся внутри цикла.
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
        // Меняем текущий контекст: дочерние узлы AST находятся внутри цикла.
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

        // Контекстно-зависимое правило: "break" допускается только внутри цикла,
        //  расположенного в пределах текущей функции.
        if (_expressionContextStack.Peek() != ExpressionContext.InsideLoop)
        {
            throw new InvalidExpressionException("The \"break\" expression is allowed only inside the loop");
        }
    }

    /// <summary>
    /// Проверяет, является ли выражение lvalue-выражением.
    /// Термин lvalue означает «значение слева от присваивания».
    /// </summary>
    private static bool IsLvalue(Expression e)
    {
        if (e is VariableAccessExpression)
        {
            return true;
        }

        if (e is ArrayAccessExpression arrayAccess)
        {
            return IsLvalue(arrayAccess.Array);
        }

        if (e is FieldAccessExpression fieldAccess)
        {
            return IsLvalue(fieldAccess.Record);
        }

        return false;
    }
}