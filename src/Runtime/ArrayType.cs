namespace PsTiger.Runtime;

/// <summary>
/// Представляет тип, являющийся массивом элементом другого типа.
/// </summary>
public sealed class ArrayType : ValueType
{
    public ArrayType(ValueType elementType)
        : base($"array of {elementType}")
    {
        ElementType = elementType;
    }

    public ValueType ElementType { get; }
}