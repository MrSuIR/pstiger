using System.Runtime.CompilerServices;

namespace PsTiger.Runtime;

public class ValueType
{
    /// <summary>
    /// Значение отсутствует.
    /// </summary>
    public static readonly ValueType Void = new();

    /// <summary>
    /// Целочисленное значение.
    /// </summary>
    public static readonly ValueType Int = new();

    /// <summary>
    /// Строковое значение.
    /// </summary>
    public static readonly ValueType String = new();

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }
}