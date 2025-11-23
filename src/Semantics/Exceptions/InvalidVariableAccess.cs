namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за некорректного обращения к переменной.
/// </summary>
public class InvalidVariableAccess : Exception
{
    public InvalidVariableAccess(string message)
        : base(message)
    {
    }
}
#pragma warning restore RCS1194