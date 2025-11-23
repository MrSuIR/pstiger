namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за некорректного вызова функции.
/// </summary>
public class InvalidFunctionCallException : Exception
{
    public InvalidFunctionCallException(string message)
        : base(message)
    {
    }
}
#pragma warning restore RCS1194