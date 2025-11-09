using System.Text;

namespace PsTiger.Lexemes;

public class Token
{
    public Token(TokenType type)
    {
        Type = type;
    }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = new TokenValue(value);
    }

    public Token(TokenType type, int value)
    {
        Type = type;
        Value = new TokenValue(value);
    }

    public TokenType Type { get; }

    public TokenValue? Value { get; }

    /// <summary>
    ///  Сравнивает токены по типу и значению.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Token other)
        {
            return Type == other.Type && Equals(Value, other.Value);
        }

        return false;
    }

    /// <summary>
    ///  Возвращает хеш от свойств токена.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Value);
    }

    /// <summary>
    /// Форматирует токен в стиле "Type (Value)".
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Type.ToString());
        if (Value != null)
        {
            sb.Append($" ({Value})");
        }

        return sb.ToString();
    }
}