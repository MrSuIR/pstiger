namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за использования выражения, которое не допускается в текущем контексте.
/// </summary>
public class InvalidExpressionException : Exception
{
    public InvalidExpressionException(string message)
        : base(message)
    {
    }
}
#pragma warning restore RCS1194