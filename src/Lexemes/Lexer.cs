namespace PsTiger.Lexemes;

public class Lexer
{
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
            return ParseIdentifier();
        }

        _scanner.Advance();
        return new Token(TokenType.Error, c.ToString());
    }

    /// <summary>
    ///  Распознаёт идентификаторы.
    ///  Правила:
    ///     identifier = letter, { letter | digit | "_" } ;
    ///     letter = a..z | A..Z ;
    ///     digit = 0..9 ;
    /// </summary>
    private Token ParseIdentifier()
    {
        string value = _scanner.Peek().ToString();
        _scanner.Advance();

        for (char c = _scanner.Peek(); char.IsAsciiLetter(c) || c == '_' || char.IsAsciiDigit(c); c = _scanner.Peek())
        {
            value += c;
            _scanner.Advance();
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