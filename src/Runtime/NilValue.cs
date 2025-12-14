namespace PsTiger.Runtime;

/// <summary>
/// Специальный тип, обозначающий несуществующую структуру.
/// </summary>
public record struct NilValue
{
    public static readonly NilValue Value = default;

    public override string ToString()
    {
        return "nil";
    }
}