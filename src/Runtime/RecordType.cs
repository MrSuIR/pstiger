namespace PsTiger.Runtime;

/// <summary>
/// Представляет тип, являющийся структурой с полями других типов.
/// </summary>
/// <remarks>
/// Список полей инициализируется после создания типа структуры.
/// </remarks>
public class RecordType : ValueType
{
    public RecordType()
        : base("record")
    {
    }

    /// <summary>
    /// Отображает имена полей на их типы данных.
    /// Хранит поля в порядке их добавления в словарь.
    /// </summary>
    public OrderedDictionary<string, ValueType> Fields { get; set; } = null!;
}