using System.Globalization;
using System.Text;

namespace PsTiger.Runtime;

internal static class ValueUtil
{
    /// <summary>
    /// Форматирует массив значений как строку.
    /// </summary>
    internal static string FormatArray(Value[] values)
    {
        StringBuilder sb = new();
        sb.Append('[');
        for (int i = 0, iMax = values.Length; i < iMax; ++i)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(values[i]);
        }

        sb.Append(']');

        return sb.ToString();
    }

    /// <summary>
    /// Форматирует поля структуры как строку.
    /// </summary>
    internal static string FormatRecord(Dictionary<string, Value> fields)
    {
        StringBuilder sb = new();
        sb.Append('{');

        bool addComma = false;
        foreach ((string name, Value value) in fields)
        {
            if (addComma)
            {
                sb.Append(", ");
            }

            addComma = true;
            sb.Append(name);
            sb.Append(": ");
            sb.Append(value);
        }

        sb.Append('}');

        return sb.ToString();
    }

    /// <summary>
    /// Печатает строковое значение в кавычках с базовым экранированием.
    /// </summary>
    internal static string EscapeStringValue(string s)
    {
        StringBuilder sb = new();
        sb.Append('"');

        foreach (char c in s)
        {
            if (c == '\n')
            {
                sb.Append(@"\n");
            }
            else if (c == '"')
            {
                sb.Append(@"\""");
            }
            else if (c == '\\')
            {
                sb.Append(@"\\");
            }
            else
            {
                if (char.IsControl(c))
                {
                    // Экранируем символ в формате \DDD, где DDD - 3-значное число с кодом ASCII.
                    sb.Append('\\');
                    sb.AppendFormat(((int)c).ToString("000", CultureInfo.InvariantCulture));
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        sb.Append('"');

        return sb.ToString();
    }
}