using System.Globalization;
using System.Text;

namespace PsTiger.Runtime;

public static class ValueUtil
{
    /// <summary>
    /// Печатает строковое значение в кавычках с базовым экранированием.
    /// </summary>
    public static string EscapeStringValue(string s)
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