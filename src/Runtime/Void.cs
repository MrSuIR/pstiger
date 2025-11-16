namespace Runtime;

/// <summary>
/// Специальный тип, обозначающий отсутствие значения.
/// </summary>
public record struct Void
{
    public static readonly Void Value = default;

    public override string ToString()
    {
        return "<void>";
    }
}