namespace PsTiger.Lexemes;

public enum TokenType
{
    /// <summary>
    /// Ключевое слово array.
    /// </summary>
    Array,

    /// <summary>
    /// Ключевое слово end.
    /// </summary>
    End,

    /// <summary>
    /// Ключевое слово in.
    /// </summary>
    In,

    /// <summary>
    /// Ключевое слово let.
    /// </summary>
    Let,

    /// <summary>
    /// Ключевое слово of.
    /// </summary>
    Of,

    /// <summary>
    /// Ключевое слово type.
    /// </summary>
    Type,

    /// <summary>
    /// Ключевое слово var.
    /// </summary>
    Var,

    /// <summary>
    /// Идентификатор.
    /// </summary>
    Identifier,

    /// <summary>
    /// Литерал (целое число или строка).
    /// </summary>
    Literal,

    /// <summary>
    /// Оператор сравнения "=".
    /// </summary>
    Equal,

    /// <summary>
    /// Оператор присваивания ":=".
    /// </summary>
    Assign,

    /// <summary>
    /// Недопустимая лексема.
    /// </summary>
    Error,

    /// <summary>
    /// Конец потока токенов.
    /// </summary>
    EndOfFile,
}