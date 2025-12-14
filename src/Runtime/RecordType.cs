using System.Text;

namespace PsTiger.Runtime;

/// <summary>
/// Представляет тип, являющийся структурой с полями других типов.
/// </summary>
public class RecordType : ValueType
{
    public RecordType(Dictionary<string, ValueType> fields)
        : base(FormatRecordName(fields))
    {
        Fields = fields;
    }

    /// <summary>
    /// Отображает имена полей на их типы данных.
    /// </summary>
    public Dictionary<string, ValueType> Fields { get; }

    /// <summary>
    /// Форматирует имя типа данных для использования в отладке.
    /// </summary>
    private static string FormatRecordName(Dictionary<string, ValueType> fields)
    {
        StringBuilder sb = new();
        sb.Append("record {");

        bool addComma = false;
        foreach ((string name, ValueType type) in fields)
        {
            if (addComma)
            {
                sb.Append(", ");
            }

            sb.Append(name);
            sb.Append(": ");
            sb.Append(type);
            addComma = true;
        }

        sb.Append('}');

        return sb.ToString();
    }
}