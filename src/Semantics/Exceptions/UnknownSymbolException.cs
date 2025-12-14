using PsTiger.Semantics.Symbols;

namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за отсутствия символа с указанным именем.
/// </summary>
public class UnknownSymbolException : Exception
{
    private UnknownSymbolException(string message)
        : base(message)
    {
    }

    public static UnknownSymbolException UndefinedVariableOrFunction(string name)
    {
        return new UnknownSymbolException($"Nor variable neither function {name} is defined in the current scope");
    }

    public static UnknownSymbolException UndefinedType(string name)
    {
        return new UnknownSymbolException($"No type {name} is defined in the current scope");
    }
}
#pragma warning restore RCS1194