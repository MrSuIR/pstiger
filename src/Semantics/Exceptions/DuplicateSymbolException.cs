using PsTiger.Semantics.Symbols;

namespace PsTiger.Semantics.Exceptions;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение из-за повторного объявления символа с тем же именем.
/// </summary>
public class DuplicateSymbolException : Exception
{
    private DuplicateSymbolException(string message)
        : base(message)
    {
    }

    public static DuplicateSymbolException DuplicateVariableOrFunction(string name)
    {
        return new DuplicateSymbolException(
            $"The variable or function name {name} is already used in the current scope");
    }

    public static DuplicateSymbolException DuplicateType(string name)
    {
        return new DuplicateSymbolException($"The type name {name} is already used in the current scope");
    }

    public static DuplicateSymbolException DuplicateField(string name)
    {
        return new DuplicateSymbolException($"The field name {name} is already used in the record");
    }
}
#pragma warning restore RCS1194