using System.Globalization;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Runtime;

/// <summary>
/// Представляет значение времени выполнения языка Tiger.
///  - Скалярные значения неизменяемы: они не меняются после создания.
///  - Массивы изменяемы: они могут быть изменены после создания.
/// </summary>
public class Value : IEquatable<Value>
{
    public static readonly Value Void = new(VoidValue.Value);
    public static readonly Value Nil = new(NilValue.Value);
    private readonly object _value;

    /// <summary>
    /// Создаёт строковое значение.
    /// </summary>
    public Value(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Создаёт целочисленное значение.
    /// </summary>
    public Value(int value)
    {
        _value = value;
    }

    private Value(object value)
    {
        _value = value;
    }

    /// <summary>
    /// Создаёт изменяемый массив заданного размера с указанным начальным значением элементов.
    /// </summary>
    public static Value NewArray(int size, Value initialValue)
    {
        Value[] array = new Value[size];
        for (int i = 0; i < size; ++i)
        {
            array[i] = initialValue.DeepCopy();
        }

        return new Value(array);
    }

    /// <summary>
    /// Создаёт изменяемую структуру без установленных полей. Структура реализуется хеш-таблицей.
    /// </summary>
    public static Value NewRecord()
    {
        Dictionary<string, Value> fields = [];
        return new Value(fields);
    }

    /// <summary>
    /// Определяет, является ли значение пустым (неопределённым).
    /// </summary>
    public bool IsVoid()
    {
        return _value switch
        {
            VoidValue => true,
            _ => false,
        };
    }

    /// <summary>
    /// Определяет, является ли значение строкой.
    /// </summary>
    public bool IsString()
    {
        return _value switch
        {
            string => true,
            _ => false,
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
    /// Определяет, является ли значение целым числом.
    /// </summary>
    public bool IsInt()
    {
        return _value switch
        {
            int => true,
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

    public Value GetElement(int index)
    {
        Value[] values = AsArray();
        CheckArrayBounds(values, index);
        return values[index];
    }

    public void SetElement(int index, Value value)
    {
        Value[] values = AsArray();
        CheckArrayBounds(values, index);
        values[index] = value;
    }

    public Value GetField(string name)
    {
        Dictionary<string, Value> fields = AsRecord();
        if (fields.TryGetValue(name, out Value? field))
        {
            return field;
        }

        throw new KeyNotFoundException($"Field '{name}' not found in record");
    }

    public void SetField(string name, Value value)
    {
        Dictionary<string, Value> fields = AsRecord();
        fields[name] = value;
    }

    /// <summary>
    /// Печатает значение для отладки.
    /// </summary>
    public override string ToString()
    {
        return _value switch
        {
            Value[] values => ValueUtil.FormatArray(values),
            Dictionary<string, Value> fields => ValueUtil.FormatRecord(fields),
            string s => ValueUtil.EscapeStringValue(s),
            int i => i.ToString(CultureInfo.InvariantCulture),
            VoidValue v => v.ToString(),
            NilValue v => v.ToString(),
            _ => throw new InvalidOperationException($"Unexpected value {_value} of type {_value.GetType()}"),
        };
    }

    /// <summary>
    /// Сравнивает два значения, возвращая истину, если текущее значение меньше переданного.
    /// </summary>
    public bool LessThan(Value other)
    {
        return _value switch
        {
            int i => i < other.AsInt(),
            string s => string.CompareOrdinal(s, other.AsString()) < 0,
            _ => throw new InvalidOperationException($"Cannot compare value {this} with {other}"),
        };
    }

    /// <summary>
    /// Сравнивает два значения, возвращая истину, если текущее значение меньше переданного.
    /// </summary>
    public bool LessThanOrEqual(Value other)
    {
        return _value switch
        {
            int i => i <= other.AsInt(),
            string s => string.CompareOrdinal(s, other.AsString()) <= 0,
            _ => throw new InvalidOperationException($"Cannot compare value {this} with {other}"),
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

        return _value switch
        {
            // Массивы равны, если указывают на один и тот же массив.
            Value[] => ReferenceEquals(other._value, _value),

            // Структуры равны, если указывают на одну и ту же структуру.
            Dictionary<string, Value> => ReferenceEquals(other._value, _value),

            // Строки сравниваются посимвольно.
            string s => other.AsString() == s,

            // Числа сравниваются по значению.
            int i => other.AsInt() == i,

            // Пустые значения всегда равны.
            VoidValue => true,

            // Несуществующая структура равна сама себе и не равна никаким другим.
            NilValue => other._value is NilValue,

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

    /// <summary>
    /// Выполняет глубокое копирование значения, если оно изменяемое.
    /// </summary>
    private Value DeepCopy()
    {
        return _value switch
        {
            Value[] values => new Value(values.Select(v => v.DeepCopy()).ToArray()),
            Dictionary<string, Value> record => new Value(new Dictionary<string, Value>(record)),
            _ => this,
        };
    }

    /// <summary>
    /// Возвращает значение как изменяемый массив.
    /// </summary>
    private Value[] AsArray()
    {
        return _value switch
        {
            Value[] values => values,
            _ => throw new InvalidOperationException($"Value {_value} is not an array"),
        };
    }

    /// <summary>
    /// Возвращает значение как изменяемую структуру, реализованную с помощью хеш-таблицы.
    /// </summary>
    private Dictionary<string, Value> AsRecord()
    {
        return _value switch
        {
            Dictionary<string, Value> dictionary => dictionary,
            _ => throw new InvalidOperationException($"Value {_value} is not a record"),
        };
    }

    private static void CheckArrayBounds(Value[] array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            throw new IndexOutOfRangeException($"Index {index} is out of array with length {array.Length}");
        }
    }
}