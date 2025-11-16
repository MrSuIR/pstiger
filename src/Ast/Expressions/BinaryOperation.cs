namespace PsTiger.Ast.Expressions;

public enum BinaryOperation
{
    /// <summary>
    /// Сложение чисел.
    /// </summary>
    Plus,

    /// <summary>
    /// Вычитание чисел.
    /// </summary>
    Minus,

    /// <summary>
    /// Умножение чисел.
    /// </summary>
    Multiply,

    /// <summary>
    /// Деление чисел.
    /// </summary>
    Divide,

    /// <summary>
    /// Логическое "ИЛИ".
    /// </summary>
    Or,

    /// <summary>
    /// Логическое "И".
    /// </summary>
    And,
}