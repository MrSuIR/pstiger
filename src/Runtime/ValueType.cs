using System.Runtime.CompilerServices;

namespace PsTiger.Runtime;

public class ValueType
{
    /// <summary>
    /// Значение отсутствует.
    /// </summary>
    public static readonly ValueType Void = new("void");

    /// <summary>
    /// Целочисленное значение.
    /// </summary>
    public static readonly ValueType Int = new("int");

    /// <summary>
    /// Строковое значение.
    /// </summary>
    public static readonly ValueType String = new("string");

    /// <summary>
    /// Несуществующая структура.
    /// </summary>
    public static readonly ValueType Nil = new("nil");

    private readonly string _name;

    protected ValueType(string name)
    {
        _name = name;
    }

    public static bool operator ==(ValueType a, ValueType b) => a.Equals(b);

    public static bool operator !=(ValueType a, ValueType b) => !a.Equals(b);

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public override string ToString()
    {
        return _name;
    }
}