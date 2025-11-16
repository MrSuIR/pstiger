using PsTiger.Ast.Expressions;
using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Execution;

public static class EvaluationUtil
{
    public static Value ApplyBinaryOperation(BinaryOperation operation, Value left, Value right)
    {
        return operation switch
        {
            BinaryOperation.Plus => new Value(left.AsInt() + right.AsInt()),
            BinaryOperation.Minus => new Value(left.AsInt() - right.AsInt()),
            BinaryOperation.Multiply => new Value(left.AsInt() * right.AsInt()),
            BinaryOperation.Divide => new Value(left.AsInt() / right.AsInt()),
            BinaryOperation.Equal => CompareValues(left, right, (i1, i2) => i1 == i2, (s1, s2) => s1 == s2),
            BinaryOperation.NotEqual => CompareValues(left, right, (i1, i2) => i1 != i2, (s1, s2) => s1 != s2),
            BinaryOperation.LessThan => CompareValues(
                left,
                right,
                (i1, i2) => i1 < i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) < 0
            ),
            BinaryOperation.GreaterThan => CompareValues(
                left,
                right,
                (i1, i2) => i1 > i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) > 0
            ),
            BinaryOperation.LessThanOrEqual => CompareValues(
                left,
                right,
                (i1, i2) => i1 <= i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) <= 0
            ),
            BinaryOperation.GreaterThanOrEqual => CompareValues(
                left,
                right,
                (i1, i2) => i1 >= i2,
                (s1, s2) => string.CompareOrdinal(s1, s2) >= 0
            ),
            _ => throw new NotImplementedException($"Unknown binary operation {operation}"),
        };
    }

    /// <summary>
    /// Сравнивает два значения, если они оба являются числами или строками.
    /// Иначе бросает исключение.
    /// </summary>
    private static Value CompareValues(
        Value left,
        Value right,
        Func<int, int, bool> compareInts,
        Func<string, string, bool> compareStrings
    )
    {
        if (left.GetValueType() != right.GetValueType())
        {
            throw new InvalidOperationException($"Cannot compare values of different types: {left} and {right}");
        }

        if (left.GetValueType() == ValueType.Int && right.GetValueType() == ValueType.Int)
        {
            bool result = compareInts(left.AsInt(), right.AsInt());
            return new Value(result ? 1 : 0);
        }

        if (left.GetValueType() == ValueType.String && right.GetValueType() == ValueType.String)
        {
            bool result = compareStrings(left.AsString(), right.AsString());
            return new Value(result ? 1 : 0);
        }

        throw new InvalidOperationException($"Values are not comparable: {left} and {right}");
    }
}