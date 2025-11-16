using PsTiger.Lexemes;

namespace PsTiger.Parsing;

/// <summary>
/// Представляет поток токенов с двумя операциями:
///  - Peek() возвращает текущий токен;
///  - Advance() переходит к следующему токену.
/// </summary>
public class TokenStream
{
    private readonly Lexer _lexer;
    private Token _nextToken;
    private readonly List<Token> _lookupBuffer;

    public TokenStream(string expression)
    {
        _lexer = new Lexer(expression);
        _nextToken = _lexer.ParseToken();
        _lookupBuffer = [];
    }

    public Token Peek()
    {
        return _nextToken;
    }

    /// <summary>
    /// Возвращает токен на N позиций вперед от текущего
    /// </summary>
    public Token Peek(int n)
    {
        if (n == 0)
        {
            return _nextToken;
        }

        // Читаем недостающие токены, если требуется.
        while (n > _lookupBuffer.Count)
        {
            Token token = _lexer.ParseToken();
            _lookupBuffer.Add(token);
        }

        return _lookupBuffer[n - 1];
    }

    public void Advance()
    {
        if (_lookupBuffer.Count > 0)
        {
            _nextToken = _lookupBuffer[0];
            _lookupBuffer.RemoveAt(0);
        }
        else
        {
            _nextToken = _lexer.ParseToken();
        }
    }
}