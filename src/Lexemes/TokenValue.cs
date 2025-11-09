using System.Globalization;

namespace PsTiger.Lexemes;

/// <summary>
/// Представляет значение токена — идентификатора или литерала.
/// </summary>
public class TokenValue
{
    private readonly object _value;

    public TokenValue(string value)
    {
        _value = value;
    }

    public TokenValue(int value)
    {
        _value = value;
    }

    /// <summary>
    /// Возвращает значение токена в виде строки.
    /// </summary>
    /// <remarks>
    ///  Имя метода перекрывает стандартный метод ToString(), поэтому добавлен `override`.
    /// </remarks>
    public override string ToString()
    {
        return _value switch
        {
            string s => s,
            int d => d.ToString(CultureInfo.InvariantCulture),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Возвращает значение токена в виде целого числа.
    /// </summary>
    public int ToInt()
    {
        return _value switch
        {
            string s => int.Parse(s, CultureInfo.InvariantCulture),
            int i => i,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    ///  Проверяет равенство значений токенов. Значения разных типов всегда считаются разными.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is TokenValue other)
        {
            return _value switch
            {
                string s => other._value is string os && s == os,
                int i => other._value is int oi && i == oi,
                _ => throw new NotImplementedException(),
            };
        }

        return false;
    }

    /// <summary>
    /// Явное определение стандартной функции GetHashCode.
    /// </summary>
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}