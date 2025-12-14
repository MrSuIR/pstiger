using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за некорректного литерала структуры.
/// </summary>
public class InvalidRecordLiteralException : Exception
{
    private InvalidRecordLiteralException(string message)
        : base(message)
    {
    }

    public static InvalidRecordLiteralException ExpectedRecordType(string name, ValueType type)
    {
        return new InvalidRecordLiteralException($"Type name {name} should refer to a record type, got {type}");
    }

    public static InvalidRecordLiteralException UnexpectedFieldName(string fieldName, ValueType recordType)
    {
        return new InvalidRecordLiteralException($"Field \"{fieldName}\" not exists in {recordType}");
    }

    public static InvalidRecordLiteralException WrongFieldInitializerIndex(
        string fieldName, int fieldIndex, int initializerIndex
    )
    {
        return new InvalidRecordLiteralException(
            $"Field \"{fieldName}\" initializer has wrong position: expected #{fieldIndex}, got #{initializerIndex}"
        );
    }

    public static InvalidRecordLiteralException MissingFieldInitializer(string fieldName, string recordTypeName)
    {
        return new InvalidRecordLiteralException(
            $"Field \"{fieldName}\" is not initialized in {recordTypeName} record literal"
        );
    }
}
#pragma warning restore RCS1194