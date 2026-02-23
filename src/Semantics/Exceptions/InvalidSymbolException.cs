namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за некорректного использования символа (функции, переменной, типа).
/// </summary>
public class InvalidSymbolException : Exception
{
    public InvalidSymbolException(string name, string expectedCategory, string actualCategory)
        : base($"Name {name} should refer to a {expectedCategory}, got {actualCategory}")
    {
        Name = name;
    }

    public string Name { get; }
}
#pragma warning restore RCS1194