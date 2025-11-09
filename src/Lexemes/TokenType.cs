namespace PsTiger.Lexemes;

public enum TokenType
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    Identifier,

    /// <summary>
    /// Недопустимая лексема.
    /// </summary>
    Error,

    /// <summary>
    /// Конец потока токенов.
    /// </summary>
    EndOfFile,
}