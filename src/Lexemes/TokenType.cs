namespace PsTiger.Lexemes;

public enum TokenType
{
    /// <summary>
    /// Ключевое слово array.
    /// </summary>
    Array,

    /// <summary>
    /// Ключевое слово break.
    /// </summary>
    Break,

    /// <summary>
    /// Ключевое слово do.
    /// </summary>
    Do,

    /// <summary>
    /// Ключевое слово else.
    /// </summary>
    Else,

    /// <summary>
    /// Ключевое слово end.
    /// </summary>
    End,

    /// <summary>
    /// Ключевое слово for.
    /// </summary>
    For,

    /// <summary>
    /// Ключевое слово function.
    /// </summary>
    Function,

    /// <summary>
    /// Ключевое слово if.
    /// </summary>
    If,

    /// <summary>
    /// Ключевое слово in.
    /// </summary>
    In,

    /// <summary>
    /// Ключевое слово let.
    /// </summary>
    Let,

    /// <summary>
    /// Ключевое слово nil.
    /// </summary>
    Nil,

    /// <summary>
    /// Ключевое слово of.
    /// </summary>
    Of,

    /// <summary>
    /// Ключевое слово then.
    /// </summary>
    Then,

    /// <summary>
    /// Ключевое слово to.
    /// </summary>
    To,

    /// <summary>
    /// Ключевое слово type.
    /// </summary>
    Type,

    /// <summary>
    /// Ключевое слово var.
    /// </summary>
    Var,

    /// <summary>
    /// Ключевое слово while.
    /// </summary>
    While,

    /// <summary>
    /// Идентификатор.
    /// </summary>
    Identifier,

    /// <summary>
    /// Литерал (целое число или строка).
    /// </summary>
    Literal,

    /// <summary>
    /// Оператор сложения "+".
    /// </summary>
    Plus,

    /// <summary>
    /// Оператор вычитания "-".
    /// </summary>
    Minus,

    /// <summary>
    /// Оператор умножения "*".
    /// </summary>
    Multiply,

    /// <summary>
    /// Оператор деления "/".
    /// </summary>
    Divide,

    /// <summary>
    /// Оператор сравнения "равно".
    /// </summary>
    Equal,

    /// <summary>
    /// Оператор сравнения "не равно".
    /// </summary>
    NotEqual,

    /// <summary>
    /// Оператор сравнения "меньше".
    /// </summary>
    LessThan,

    /// <summary>
    /// Оператор сравнения "больше".
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Оператор сравнения "меньше или равно".
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Оператор сравнения "больше или равно".
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Оператор "и".
    /// </summary>
    And,

    /// <summary>
    /// Оператор "или".
    /// </summary>
    Or,

    /// <summary>
    /// Оператор присваивания ":=".
    /// </summary>
    Assign,

    /// <summary>
    /// Точка.
    /// </summary>
    Dot,

    /// <summary>
    /// Запятая.
    /// </summary>
    Comma,

    /// <summary>
    /// Двоеточие.
    /// </summary>
    Colon,

    /// <summary>
    /// Точка с запятой.
    /// </summary>
    Semicolon,

    /// <summary>
    /// Открывающая круглая скобка.
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Закрывающая круглая скобка.
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Открывающая квадратная скобка.
    /// </summary>
    OpenBracket,

    /// <summary>
    /// Закрывающая квадратная скобка.
    /// </summary>
    CloseBracket,

    /// <summary>
    /// Открывающая фигурная скобка.
    /// </summary>
    OpenBrace,

    /// <summary>
    /// Закрывающая фигурная скобка.
    /// </summary>
    CloseBrace,

    /// <summary>
    /// Недопустимая лексема.
    /// </summary>
    Error,

    /// <summary>
    /// Конец потока токенов.
    /// </summary>
    EndOfFile,
}