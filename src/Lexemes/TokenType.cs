namespace PsTiger.Lexemes;

public enum TokenType
{
    /// <summary>
    /// Ключевое слово array.
    /// </summary>
    Array,

    /// <summary>
    /// Ключевое слово of.
    /// </summary>
    Of,

    /// <summary>
    /// Ключевое слово type.
    /// </summary>
    Type,

    /// <summary>
    /// Идентификатор.
    /// </summary>
    Identifier,

    /// <summary>
    /// Оператор сравнения "=".
    /// </summary>
    Equal,

    /// <summary>
    /// Недопустимая лексема.
    /// </summary>
    Error,

    /// <summary>
    /// Конец потока токенов.
    /// </summary>
    EndOfFile,
}