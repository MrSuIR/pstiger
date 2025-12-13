using PsTiger.Semantics.Symbols;

namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за повторного объявления символа с тем же именем.
/// </summary>
public class DuplicateSymbolException : Exception
{
    public DuplicateSymbolException(string category, string name)
        : base($"The {category} name {name} is already defined in the current scope")
    {
    }
}
#pragma warning restore RCS1194