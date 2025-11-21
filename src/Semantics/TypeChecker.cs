using PsTiger.Ast;
using PsTiger.Ast.Expressions;

using ValueType = PsTiger.Runtime.ValueType;

namespace Semantics;

/// <summary>
/// Проверяет корректность программы с точки зрения совместимости типов данных.
/// </summary>
/// <exception cref="TypeErrorException">Бросается при несоответствии типов данных.</exception>
public class TypeChecker : IAstVisitor
{
    /// <summary>
    /// В стек временно складываются результаты вывода типа текущей операции.
    /// </summary>
    private readonly Stack<ValueType> _types = [];

    /// <summary>
    /// Литерал всегда имеет определённый тип.
    /// </summary>
    public void Visit(LiteralExpression e)
    {
        _types.Push(e.Value.GetValueType());
    }

    /// <summary>
    /// Выполняет проверки типов для бинарных операций:
    /// 1. Арифметические и логические операции выполняются над целыми числами и возвращают число.
    /// 2. Операции сравнения выполняются над двумя числами либо двумя строками и возвращают тот же тип.
    /// </summary>
    public void Visit(BinaryOperationExpression e)
    {
        e.Left.Accept(this);
        ValueType left = _types.Pop();

        e.Right.Accept(this);
        ValueType right = _types.Pop();

        ValueType? resultType = GetBinaryOperationResultType(e.Operation, left, right);
        if (resultType == null)
        {
            throw new TypeErrorException($"Binary operation {e.Operation} is not allowed for types {left} and {right}");
        }

        _types.Push(resultType.Value);
    }

    /// <summary>
    /// Выполняет проверки типов для последовательности выражений:
    ///  1. Пустая последовательность `()` не возвращает значения.
    ///  2. Непустая последовательность возвращает результат последнего выражения.
    ///  3. Все выражения в последовательности должны быть соблюдать семантику языка.
    /// </summary>
    public void Visit(SequenceExpression e)
    {
        _types.Push(ValueType.Void);
        foreach (Expression nested in e.Sequence)
        {
            _types.Pop();
            nested.Accept(this);
        }
    }

    /// <summary>
    /// Выполняет проверки типов для унарного минуса.
    /// Унарный минус применяется только к целым числам и возвращает целое число.
    /// </summary>
    public void Visit(UnaryMinusExpression e)
    {
        e.Operand.Accept(this);
        ValueType operandType = _types.Pop();
        if (operandType != ValueType.Int)
        {
            throw new TypeErrorException($"Unary minus operation is not allowed for type {operandType}");
        }

        _types.Push(operandType);
    }

    private ValueType? GetBinaryOperationResultType(BinaryOperation operation, ValueType left, ValueType right)
    {
        switch (operation)
        {
            case BinaryOperation.Add:
            case BinaryOperation.Substract:
            case BinaryOperation.Multiply:
            case BinaryOperation.Divide:
            case BinaryOperation.Or:
            case BinaryOperation.And:
                if (left == ValueType.Int && right == ValueType.Int)
                {
                    return ValueType.Int;
                }

                return null;
            case BinaryOperation.LessThan:
            case BinaryOperation.GreaterThan:
            case BinaryOperation.LessThanOrEqual:
            case BinaryOperation.GreaterThanOrEqual:
            case BinaryOperation.Equal:
            case BinaryOperation.NotEqual:
                if (left == ValueType.Int && right == ValueType.Int)
                {
                    return ValueType.Int;
                }

                if (left == ValueType.String && right == ValueType.String)
                {
                    return ValueType.String;
                }

                return null;

            default:
                throw new ArgumentException($"Unknown binary operation {operation}");
        }
    }
}