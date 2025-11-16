using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Execution;

public static class EvaluationUtil
{
    /// <summary>
    /// Сравнивает два значения, если они оба являются числами или строками.
    /// Иначе бросает исключение.
    /// </summary>
    public static Value CompareValues(
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