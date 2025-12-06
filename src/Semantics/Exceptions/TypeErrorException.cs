namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за несовместимых типов данных в программе.
/// </summary>
public class TypeErrorException : Exception
{
    public TypeErrorException(string message)
        : base(message)
    {
    }

    public TypeErrorException(string category, ValueType expected, ValueType actual)
        : base($"Type mismatch: {category} must be an {expected}, got {actual}")
    {
    }
}
#pragma warning restore RCS1194