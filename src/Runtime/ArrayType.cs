namespace PsTiger.Runtime;

/// <summary>
/// Представляет тип, являющийся массивом элементом другого типа.
/// </summary>
/// <remarks>
/// Тип элемента массива инициализируется после создания типа массива.
/// </remarks>
public sealed class ArrayType : ValueType
{
    public ArrayType()
        : base("array")
    {
    }

    public ValueType ElementType { get; set; } = null!;
}