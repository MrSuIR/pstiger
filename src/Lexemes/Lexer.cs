using System.Globalization;
using System.Text;

namespace PsTiger.Lexemes;

public class Lexer
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        {
            "array", TokenType.Array
        },
        {
            "break", TokenType.Break
        },
        {
            "do", TokenType.Do
        },
        {
            "end", TokenType.End
        },
        {
            "in", TokenType.In
        },
        {
            "let", TokenType.Let
        },
        {
            "of", TokenType.Of
        },
        {
            "type", TokenType.Type
        },
        {
            "var", TokenType.Var
        },
        {
            "while", TokenType.While
        },
    };

    private readonly TextScanner _scanner;

    public Lexer(string code)
    {
        _scanner = new TextScanner(code);
    }

    public Token ParseToken()
    {
        SkipWhiteSpaces();

        if (_scanner.IsEnd())
        {
            return new Token(TokenType.EndOfFile);
        }

        char c = _scanner.Peek();
        if (char.IsAsciiLetter(c))
        {
            return ParseIdentifierOrKeyword();
        }

        if (char.IsAsciiDigit(c))
        {
            return ParseIntLiteral();
        }

        // Разбор операторов, разделителей и скобок.
        switch (c)
        {
            case '=':
                _scanner.Advance();
                return new Token(TokenType.Equal);

            case ':':
                if (_scanner.Peek(1) == '=')
                {
                    _scanner.Advance();
                    _scanner.Advance();
                    return new Token(TokenType.Assign);
                }

                break;

            case '(':
                _scanner.Advance();
                return new Token(TokenType.OpenParenthesis);

            case ')':
                _scanner.Advance();
                return new Token(TokenType.CloseParenthesis);
        }

        _scanner.Advance();
        return new Token(TokenType.Error, c.ToString());
    }

    /// <summary>
    /// Разбирает литерал целого числа. Возвращает лексему Error, если число выходит за пределы типа данных int.
    /// </summary>
    private Token ParseIntLiteral()
    {
        // NOTE: Сохраняем цифры в буфер, чтобы не терять данные при возврате лексемы Error.
        StringBuilder sb = new();
        for (char ch = _scanner.Peek(); char.IsAsciiDigit(ch); ch = _scanner.Peek())
        {
            sb.Append(ch);
            _scanner.Advance();
        }

        string digits = sb.ToString();
        if (int.TryParse(digits, CultureInfo.InvariantCulture, out int value))
        {
            return new Token(TokenType.Literal, value);
        }

        return new Token(TokenType.Error, digits);
    }

    /// <summary>
    ///  Распознаёт идентификаторы.
    ///  Правила:
    ///     identifier = letter, { letter | digit | "_" } ;
    ///     letter = a..z | A..Z ;
    ///     digit = 0..9 ;
    /// </summary>
    private Token ParseIdentifierOrKeyword()
    {
        string value = _scanner.Peek().ToString();
        _scanner.Advance();

        for (char c = _scanner.Peek(); char.IsAsciiLetter(c) || c == '_' || char.IsAsciiDigit(c); c = _scanner.Peek())
        {
            value += c;
            _scanner.Advance();
        }

        // Проверяем на совпадение с ключевым словом (с учётом регистра).
        if (Keywords.TryGetValue(value, out TokenType type))
        {
            return new Token(type);
        }

        // Возвращаем токен идентификатора.
        return new Token(TokenType.Identifier, value);
    }

    /// <summary>
    ///  Пропускает пробельные символы, пока не встретит иной символ.
    /// </summary>
    private void SkipWhiteSpaces()
    {
        while (char.IsWhiteSpace(_scanner.Peek()))
        {
            _scanner.Advance();
        }
    }
}