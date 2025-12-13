namespace PsTiger.Runtime;

public sealed class ArrayType : ValueType
{
    public ArrayType(ValueType elementType)
        : base($"array of {elementType}")
    {
        this.ElementType = elementType;
    }

    public ValueType ElementType { get; }
}