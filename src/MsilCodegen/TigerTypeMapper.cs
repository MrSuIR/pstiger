using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.MsilCodegen;

/// <summary>
/// Отображает типы языка Tiger на соответствующие им типы .NET.
/// </summary>
public class TigerTypeMapper
{
    private readonly Dictionary<ValueType, Type> _typesMap = [];

    /// <summary>
    /// Отображает тип языка Tiger на соответствующий ему тип .NET.
    /// </summary>
    /// <remarks>
    /// Кэширует результаты отображения в словаре.
    /// </remarks>
    public Type MapType(ValueType type)
    {
        if (!_typesMap.TryGetValue(type, out Type? result))
        {
            result = MapTypeImpl(type);
            _typesMap.Add(type, result);
        }

        return result;
    }

    private Type MapTypeImpl(ValueType type)
    {
        if (type == ValueType.Void)
        {
            return typeof(void);
        }

        if (type == ValueType.Int)
        {
            return typeof(int);
        }

        if (type == ValueType.String)
        {
            return typeof(string);
        }

        if (type == ValueType.Nil)
        {
            // В Tiger значение nil используется для инициализации переменных с типом структуры.
            // В .NET структурам соответствует тип object.
            return typeof(object);
        }

        if (type is ArrayType arrayType)
        {
            return MapArrayTypeImpl(arrayType);
        }

        if (type is RecordType recordType)
        {
            return MapRecordTypeImpl(recordType);
        }

        throw new NotSupportedException($"Tiger type {type} cannot be converted into .NET type");
    }

    /// <summary>
    /// Отображает массивы типа T языка Tiger на тип T[] в .NET
    /// </summary>
    private Type MapArrayTypeImpl(ArrayType arrayType)
    {
        Type elementType = MapType(arrayType);
        return elementType.MakeArrayType();
    }

    private Type MapRecordTypeImpl(RecordType recordType)
    {
        throw new NotImplementedException($"Tiger record type {recordType} cannot be converted into .NET type yet");
    }
}