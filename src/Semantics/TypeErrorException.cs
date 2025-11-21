namespace Semantics;

using ValueType = PsTiger.Runtime.ValueType;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
public class TypeErrorException : Exception
{
    public TypeErrorException(string message)
        : base(message)
    {
    }
}
#pragma warning restore RCS1194