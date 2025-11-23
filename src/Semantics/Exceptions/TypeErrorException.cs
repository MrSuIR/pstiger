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
}
#pragma warning restore RCS1194