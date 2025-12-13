namespace PsTiger.Runtime;

/// <summary>
/// Специальный тип, обозначающий отсутствие значения.
/// </summary>
public record struct VoidValue
{
    public static readonly VoidValue Value = default;

    public override string ToString()
    {
        return "<void>";
    }
}