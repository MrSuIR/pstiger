using PsTiger.Lexemes;

namespace PsTiger.Parsing;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
public class UnexpectedLexemeException : Exception
{
    public UnexpectedLexemeException(Token actual, TokenType expected)
        : base($"Unexpected lexeme {actual} where expected {expected}")
    {
    }

    public UnexpectedLexemeException(Token actual, IEnumerable<TokenType> expected)
        : base($"Unexpected lexeme {actual} where expected one of {string.Join(", ", expected)}")
    {
    }
}
#pragma warning restore RCS1194