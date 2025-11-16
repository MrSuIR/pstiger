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
            "else", TokenType.Else
        },
        {
            "end", TokenType.End
        },
        {
            "for", TokenType.For
        },
        {
            "function", TokenType.Function
        },
        {
            "if", TokenType.If
        },
        {
            "in", TokenType.In
        },
        {
            "let", TokenType.Let
        },
        {
            "nil", TokenType.Nil
        },
        {
            "of", TokenType.Of
        },
        {
            "then", TokenType.Then
        },
        {
            "to", TokenType.To
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

    private static readonly Dictionary<char, char> SimpleEscapes = new()
    {
        {
            'n', '\n'
        },
        {
            't', '\t'
        },
        {
            '"', '\"'
        },
        {
            '\\', '\\'
        },
    };

    private static readonly Dictionary<char, char> CaretEscapes = new()
    {
        {
            '@', '\0'
        },
        {
            'A', '\x01'
        },
        {
            'B', '\x02'
        },
        {
            'C', '\x03'
        },
        {
            'D', '\x04'
        },
        {
            'E', '\x05'
        },
        {
            'F', '\x06'
        },
        {
            'G', '\x07'
        },
        {
            'H', '\x08'
        },
        {
            'I', '\x09'
        },
        {
            'J', '\x0A'
        },
        {
            'K', '\x0B'
        },
        {
            'L', '\x0C'
        },
        {
            'M', '\x0D'
        },
        {
            'N', '\x0E'
        },
        {
            'O', '\x0F'
        },
        {
            'P', '\x10'
        },
        {
            'Q', '\x11'
        },
        {
            'R', '\x12'
        },
        {
            'S', '\x13'
        },
        {
            'T', '\x14'
        },
        {
            'U', '\x15'
        },
        {
            'V', '\x16'
        },
        {
            'W', '\x17'
        },
        {
            'X', '\x18'
        },
        {
            'Y', '\x19'
        },
        {
            'Z', '\x1A'
        },
        {
            '[', '\x1B'
        },
        {
            '\\', '\x1C'
        },
        {
            ']', '\x1D'
        },
        {
            '^', '\x1E'
        },
        {
            '_', '\x1F'
        },
        {
            '?', '\x7F'
        },
    };

    private readonly TextScanner _scanner;

    public Lexer(string code)
    {
        _scanner = new TextScanner(code);
    }

    public Token ParseToken()
    {
        SkipWhiteSpacesAndComments();

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

        if (c == '"')
        {
            return ParseStringLiteral();
        }

        // Разбор операторов, разделителей и скобок.
        switch (c)
        {
            case '+':
                _scanner.Advance();
                return new Token(TokenType.Plus);

            case '-':
                _scanner.Advance();
                return new Token(TokenType.Minus);

            case '*':
                _scanner.Advance();
                return new Token(TokenType.Multiply);

            case '/':
                _scanner.Advance();
                return new Token(TokenType.Divide);

            case '=':
                _scanner.Advance();
                return new Token(TokenType.Equal);

            case '<':
                _scanner.Advance();
                if (_scanner.Peek() == '>')
                {
                    _scanner.Advance();
                    return new Token(TokenType.NotEqual);
                }

                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.LessThanOrEqual);
                }

                return new Token(TokenType.LessThan);

            case '>':
                _scanner.Advance();
                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.GreaterThanOrEqual);
                }

                return new Token(TokenType.GreaterThan);

            case '&':
                _scanner.Advance();
                return new Token(TokenType.And);

            case '|':
                _scanner.Advance();
                return new Token(TokenType.Or);

            case '.':
                _scanner.Advance();
                return new Token(TokenType.Dot);

            case ',':
                _scanner.Advance();
                return new Token(TokenType.Comma);

            case ':':
                _scanner.Advance();
                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.Assign);
                }

                return new Token(TokenType.Colon);

            case ';':
                _scanner.Advance();
                return new Token(TokenType.Semicolon);

            case '(':
                _scanner.Advance();
                return new Token(TokenType.OpenParenthesis);

            case ')':
                _scanner.Advance();
                return new Token(TokenType.CloseParenthesis);

            case '[':
                _scanner.Advance();
                return new Token(TokenType.OpenBracket);

            case ']':
                _scanner.Advance();
                return new Token(TokenType.CloseBracket);

            case '{':
                _scanner.Advance();
                return new Token(TokenType.OpenBrace);

            case '}':
                _scanner.Advance();
                return new Token(TokenType.CloseBrace);
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
    /// Разбирает литерал строки.
    /// Возвращает лексему Error, если:
    ///   1) встречает неизвестную escape-последовательность
    ///   2) у строки нет закрывающей кавычки
    /// </summary>
    private Token ParseStringLiteral()
    {
        StringBuilder valueBuilder = new();
        bool hasError = false;

        // Пропускаем открывающую кавычку.
        _scanner.Advance();

        for (char c = _scanner.Peek(); c != '"'; c = _scanner.Peek())
        {
            if (_scanner.IsEnd())
            {
                return new Token(TokenType.Error, valueBuilder.ToString());
            }

            if (c == '\\')
            {
                if (!DecodeEscapeSequence(valueBuilder))
                {
                    hasError = true;
                    valueBuilder.Append('\\');
                }
            }
            else
            {
                valueBuilder.Append(c);
                _scanner.Advance();
            }
        }

        // Пропускаем закрывающую кавычку.
        _scanner.Advance();

        if (hasError)
        {
            return new Token(TokenType.Error, valueBuilder.ToString());
        }

        return new Token(TokenType.Literal, valueBuilder.ToString());
    }

    private bool DecodeEscapeSequence(StringBuilder valueBuilder)
    {
        // Предполагаем, что первый символ — обратный слеш "\".
        _scanner.Advance();

        char ch1 = _scanner.Peek();

        // Разбор простой escape-последовательности: "\n", "\"" и так далее.
        if (SimpleEscapes.TryGetValue(ch1, out char unescaped1))
        {
            _scanner.Advance();
            valueBuilder.Append(unescaped1);
            return true;
        }

        // Разбор escape-последовательности в каретной нотации: "\^@", "\^C" и так далее.
        if (ch1 == '^')
        {
            char ch2 = _scanner.Peek(1);
            if (CaretEscapes.TryGetValue(ch2, out char unescaped2))
            {
                _scanner.Advance();
                _scanner.Advance();
                valueBuilder.Append(unescaped2);
                return true;
            }
        }

        // Разбор escape-последовательности в нотации "\DDD", где D — десятичная цифра,
        //  а DDD — код символа ASCII (число от 0 до 127 включительно).
        if (char.IsAsciiDigit(ch1))
        {
            char ch2 = _scanner.Peek(1);
            if (char.IsAsciiDigit(ch2))
            {
                char ch3 = _scanner.Peek(2);
                if (char.IsAsciiDigit(ch3))
                {
                    int code = (ch1 - '0') * 100 + (ch2 - '0') * 10 + (ch3 - '0');
                    if (code <= 127)
                    {
                        _scanner.Advance();
                        _scanner.Advance();
                        _scanner.Advance();
                        valueBuilder.Append((char)code);
                        return true;
                    }
                }
            }
        }

        // Пропуск любой последовательности пробельных символов между двумя обратными слэшами '\', например: "\   \"
        if (char.IsWhiteSpace(ch1))
        {
            int skipCount = 1;
            while (char.IsWhiteSpace(_scanner.Peek(skipCount)))
            {
                ++skipCount;
            }

            if (_scanner.Peek(skipCount) == '\\')
            {
                for (int i = 0; i <= skipCount; ++i)
                {
                    _scanner.Advance();
                }

                return true;
            }
        }

        return false;
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
    /// Пропускает пробельные символы и комментарии, пока не встретит лексему.
    /// </summary>
    private void SkipWhiteSpacesAndComments()
    {
        do
        {
            SkipWhiteSpaces();
        }
        while (SkipMultiLineComment());
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

    /// <summary>
    /// Пропускает комментарии, в том числе вложенные (за счёт рекурсии).
    /// </summary>
    private bool SkipMultiLineComment()
    {
        if (_scanner.Peek() == '/' && _scanner.Peek(1) == '*')
        {
            _scanner.Advance(); // Пропускаем '/'.
            _scanner.Advance(); // Пропускаем '*'.

            while (!_scanner.IsEnd())
            {
                if (_scanner.Peek() == '*' && _scanner.Peek(1) == '/')
                {
                    // Встретили конец комментария.
                    break;
                }

                // Пропускаем вложенный комментарий либо один последующий символ.
                if (!SkipMultiLineComment())
                {
                    _scanner.Advance();
                }
            }

            _scanner.Advance(); // Пропускаем '*'.
            _scanner.Advance(); // Пропускаем '/'.
            return true;
        }

        return false;
    }
}