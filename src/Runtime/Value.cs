using System.Globalization;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Runtime;

public class Value : IEquatable<Value>
{
    public static readonly Value Void = new(VoidType.Value);

    private readonly object _value;

    public Value(string value)
    {
        _value = value;
    }

    public Value(int value)
    {
        _value = value;
    }

    private Value(VoidType value)
    {
        _value = value;
    }

    /// <summary>
    /// Возвращает тип значения.
    /// </summary>
    public ValueType GetValueType()
    {
        return _value switch
        {
            string => ValueType.String,
            int => ValueType.Int,
            VoidType => ValueType.Void,
            _ => throw new InvalidOperationException($"Unexpected value {_value} of type {_value.GetType()}"),
        };
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

    /// <summary>
    /// Печатает значение для отладки.
    /// </summary>
    public override string ToString()
    {
        return _value switch
        {
            string s => ValueUtil.EscapeStringValue(s),
            int i => i.ToString(CultureInfo.InvariantCulture),
            VoidType v => v.ToString(),
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

        if (GetValueType() != other.GetValueType())
        {
            return false;
        }

        return _value switch
        {
            string s => other.AsString() == s,
            int i => other.AsInt() == i,
            VoidType => true,
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