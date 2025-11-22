using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;

namespace Semantics;

/// <summary>
/// Проверяет корректность программы с точки зрения использования функций.
/// </summary>
/// <exception cref="TypeErrorException">Бросается при несоответствии типов данных.</exception>
public class FunctionsChecker : IAstVisitor
{
    /// <summary>
    /// Словарь встроенных функций языка.
    /// </summary>
    private readonly IReadOnlyDictionary<string, BuiltinFunction> _builtins;

    public FunctionsChecker(IReadOnlyDictionary<string, BuiltinFunction> builtins)
    {
        _builtins = builtins;
    }

    public void Visit(LiteralExpression e)
    {
    }

    public void Visit(BinaryOperationExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
    }

    public void Visit(SequenceExpression e)
    {
        foreach (Expression nested in e.Sequence)
        {
            nested.Accept(this);
        }
    }

    public void Visit(UnaryMinusExpression e)
    {
        e.Operand.Accept(this);
    }

    public void Visit(FunctionCallExpression e)
    {
        if (!_builtins.TryGetValue(e.Name, out BuiltinFunction? function))
        {
            throw new InvalidFunctionCallException($"Function {e.Name} is not defined");
        }

        if (e.Arguments.Count != function.Parameters.Count)
        {
            throw new InvalidFunctionCallException(
                $"Function {e.Name} requires {function.Parameters.Count} arguments, got {e.Arguments.Count}"
            );
        }

        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }
    }
}