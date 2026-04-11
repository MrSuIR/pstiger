using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.MsilCodegen;

/// <summary>
/// Создаёт типы .NET для структур (records), объявленных в программе на Tiger.
/// </summary>
public class RecordTypeFactory
{
    private readonly ModuleBuilder _moduleBuilder;
    private int _nextRecordId = 1;
    private readonly Dictionary<RecordType, TypeBuilder> _recordTypeMap = [];
    private readonly Dictionary<RecordType, RecordTypeData> _recordTypeDataMap = [];

    public RecordTypeFactory(ModuleBuilder moduleBuilder)
    {
        _moduleBuilder = moduleBuilder;
    }

    /// <summary>
    /// Создаёт тип для структуры (record), заполняет его поля и генерирует конструктор,
    ///  принимающий все поля в порядке их объявления.
    /// </summary>
    public Type GetOrCreateRecordType(RecordType recordType, Func<ValueType, Type> mapToNetType)
    {
        if (_recordTypeMap.TryGetValue(recordType, out TypeBuilder? existingTypeBuilder))
        {
            return existingTypeBuilder;
        }

        // Создаём тип структуры с именем вида "Record{N}" и добавляем в словарь.
        TypeBuilder typeBuilder = _moduleBuilder.DefineType(
            GenerateNetRecordName(),
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class | TypeAttributes.AutoLayout
        );
        _recordTypeMap.Add(recordType, typeBuilder);

        // Добавляем в структуру все её поля и запоминаем список полей.
        List<FieldBuilder> fields = new(capacity: recordType.Fields.Count);
        foreach ((string fieldName, ValueType fieldType) in recordType.Fields)
        {
            Type fieldNetType = mapToNetType(fieldType);
            fields.Add(typeBuilder.DefineField(fieldName, fieldNetType, FieldAttributes.Public));
        }

        // Добавляем конструктор, принимающий все поля в порядке их объявления.
        ConstructorBuilder constructor = GenerateRecordConstructor(typeBuilder, fields);

        _recordTypeDataMap[recordType] = new RecordTypeData(
            fields.ToDictionary(field => field.Name),
            constructor
        );

        return typeBuilder;
    }

    /// <summary>
    /// Возвращает конструктор структуры, принимающий все поля в порядке их объявления.
    /// </summary>
    public ConstructorInfo GetRecordConstructor(RecordType recordType)
    {
        return _recordTypeDataMap[recordType].Constructor;
    }

    /// <summary>
    /// Возвращает поле структуры по его имени.
    /// </summary>
    public FieldInfo GetRecordField(RecordType recordType, string fieldName)
    {
        return _recordTypeDataMap[recordType].Fields[fieldName];
    }

    /// <summary>
    /// Завершает создание всех типов структур.
    /// </summary>
    public void FinishCreateTypes()
    {
        foreach (TypeBuilder typeBuilder in _recordTypeMap.Values)
        {
            typeBuilder.CreateType();
        }
    }

    /// <summary>
    /// Добавляет для структуры конструктор, принимающий все её поля в порядке их объявления.
    /// </summary>
    private ConstructorBuilder GenerateRecordConstructor(TypeBuilder typeBuilder, List<FieldBuilder> fields)
    {
        ConstructorBuilder constructor = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.HasThis,
            fields.Select(field => field.FieldType).ToArray()
        );

        ILGenerator il = constructor.GetILGenerator();

        // Вызов конструктора object (базовый тип всех объектов .NET).
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);

        // Конструктор поочерёдно сохраняет каждый аргумент в соответствующее поле структуры.
        // Аргумент с номером 0 - это this, а далее следуют значения для полей структуры.
        int argIndex = 1;
        foreach (FieldBuilder field in fields)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg, argIndex++);
            il.Emit(OpCodes.Stfld, field);
        }

        il.Emit(OpCodes.Ret);

        return constructor;
    }

    private string GenerateNetRecordName()
    {
        int recordId = _nextRecordId++;
        return "Record" + recordId.ToString(CultureInfo.InvariantCulture);
    }

    private class RecordTypeData(Dictionary<string, FieldBuilder> fields, ConstructorBuilder constructor)
    {
        public Dictionary<string, FieldBuilder> Fields => fields;

        public ConstructorBuilder Constructor => constructor;
    }
}