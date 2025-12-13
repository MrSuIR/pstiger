using System.Globalization;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Runtime;

public class Value : IEquatable<Value>
{
    public static readonly Value Void = new(VoidValue.Value);
    private readonly object _value;

    public Value(string value)
    {
        _value = value;
    }

    public Value(int value)
    {
        _value = value;
    }

    private Value(VoidValue value)
    {
        _value = value;
    }

    /// <summary>
    /// Возвращает значение как строку либо бросает исключение.
    /// </summary>
    public string AsString()
    {
        return _value switch
        {
            string s => s,
            _ => throw new InvalidOperationException($"Value {_value} is not a string"),
        };
    }

    public bool IsInt()
    {
        return _value switch
        {
            int i => true,
            _ => false,
        };
    }

    /// <summary>
    /// Возвращает значение как целое число либо бросает исключение.
    /// </summary>
    public int AsInt()
    {
        return _value switch
        {
            int i => i,
            _ => throw new InvalidOperationException($"Value {_value} is not an integer"),
        };
    }

    public bool IsString()
    {
        return _value switch
        {
            string s => true,
            _ => false,
        };
    }

    /// <summary>
    /// Печатает значение для отладки.
    /// </summary>
    public override string ToString()
    {
        return _value switch
        {
            string s => ValueUtil.EscapeStringValue(s),
            int i => i.ToString(CultureInfo.InvariantCulture),
            VoidValue v => v.ToString(),
            _ => throw new InvalidOperationException($"Unexpected value {_value} of type {_value.GetType()}"),
        };
    }

    /// <summary>
    /// Сравнивает на равенство два значения.
    /// </summary>
    public bool Equals(Value? other)
    {
        if (other is null)
        {
            return false;
        }

        if (_value.GetType() != other._value.GetType())
        {
            return false;
        }

        return _value switch
        {
            string s => other.AsString() == s,
            int i => other.AsInt() == i,
            VoidValue => true,
            _ => throw new NotImplementedException(),
        };
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}