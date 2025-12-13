using PsTiger.Semantics.Symbols;

namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за отсутствия символа с указанным именем.
/// </summary>
public class UnknownSymbolException : Exception
{
    public UnknownSymbolException(string category, string name)
        : base($"The {category} name {name} is not defined in the current context")
    {
    }
}
#pragma warning restore RCS1194