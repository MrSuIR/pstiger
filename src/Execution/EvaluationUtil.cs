using PsTiger.Ast.Expressions;
using PsTiger.Runtime;

namespace PsTiger.Execution;

public static class EvaluationUtil
{
    public static Value ApplyBinaryOperation(
        BinaryOperation operation, Func<Value> evaluateLeft, Func<Value> evaluateRight
    )
    {
        return operation switch
        {
            BinaryOperation.Add => ApplyArithmeticOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 + i2
            ),
            BinaryOperation.Substract => ApplyArithmeticOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 - i2
            ),
            BinaryOperation.Multiply => ApplyArithmeticOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 * i2
            ),
            BinaryOperation.Divide => ApplyArithmeticOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 / i2
            ),
            BinaryOperation.Equal => ApplyComparisonOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 == i2,
                (s1, s2) => s1 == s2
            ),
            BinaryOperation.NotEqual => ApplyComparisonOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 != i2,
                (s1, s2) => s1 != s2
            ),
            BinaryOperation.LessThan => ApplyComparisonOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 < i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) < 0
            ),
            BinaryOperation.GreaterThan => ApplyComparisonOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 > i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) > 0
            ),
            BinaryOperation.LessThanOrEqual => ApplyComparisonOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 <= i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) <= 0
            ),
            BinaryOperation.GreaterThanOrEqual => ApplyComparisonOperation(
                evaluateLeft,
                evaluateRight,
                (i1, i2) => i1 >= i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) >= 0
            ),
            BinaryOperation.Or => ApplyLogicalOr(
                evaluateLeft,
                evaluateRight
            ),
            BinaryOperation.And => ApplyLogicalAnd(
                evaluateLeft,
                evaluateRight
            ),
            _ => throw new NotImplementedException($"Unknown binary operation {operation}"),
        };
    }

    /// <summary>
    /// Выполняет арифметическую операцию, если оба операнда являются числами.
    /// Иначе бросает исключение.
    /// </summary>
    private static Value ApplyArithmeticOperation(
        Func<Value> evaluateLeft, Func<Value> evaluateRight, Func<int, int, int> operation
    )
    {
        int left = evaluateLeft().AsInt();
        int right = evaluateRight().AsInt();
        return new Value(operation(left, right));
    }

    /// <summary>
    /// Сравнивает два операнда, если они оба являются числами или строками.
    /// Иначе бросает исключение.
    /// </summary>
    private static Value ApplyComparisonOperation(
        Func<Value> evaluateLeft,
        Func<Value> evaluateRight,
        Func<int, int, bool> compareInts,
        Func<string, string, bool> compareStrings
    )
    {
        Value left = evaluateLeft();
        Value right = evaluateRight();

        if (left.IsInt() && right.IsInt())
        {
            bool result = compareInts(left.AsInt(), right.AsInt());
            return new Value(result ? 1 : 0);
        }

        if (left.IsString() && right.IsString())
        {
            bool result = compareStrings(left.AsString(), right.AsString());
            return new Value(result ? 1 : 0);
        }

        throw new InvalidOperationException($"Values are not comparable: {left} and {right}");
    }

    /// <summary>
    /// Вычисляет логическое "ИЛИ".
    /// Реализует вычисление по короткой схеме (short-circuit evaluation).
    /// </summary>
    private static Value ApplyLogicalOr(Func<Value> evaluateLeft, Func<Value> evaluateRight)
    {
        int left = evaluateLeft().AsInt();
        if (left != 0)
        {
            return new Value(1);
        }

        int result = (evaluateRight().AsInt() != 0) ? 1 : 0;
        return new Value(result);
    }

    /// <summary>
    /// Вычисляет логическое "И".
    /// Реализует вычисление по короткой схеме (short-circuit evaluation).
    /// </summary>
    private static Value ApplyLogicalAnd(Func<Value> evaluateLeft, Func<Value> evaluateRight)
    {
        int left = evaluateLeft().AsInt();
        if (left == 0)
        {
            return new Value(0);
        }

        int result = (evaluateRight().AsInt() != 0) ? 1 : 0;
        return new Value(result);
    }
}