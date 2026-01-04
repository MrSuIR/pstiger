namespace PsTiger.VirtualMachine;

public enum InstructionCode
{
    /// <summary>
    /// Добавляет значение в стек значений.
    /// </summary>
    Push,

    /// <summary>
    /// Складывает два числа на стеке вычислений.
    /// </summary>
    Add,

    /// <summary>
    /// Вычитает два числа на стеке вычислений.
    /// </summary>
    Subtract,

    /// <summary>
    /// Умножает два числа на стеке вычислений.
    /// </summary>
    Multiply,

    /// <summary>
    /// Делит два числа на стеке вычислений.
    /// </summary>
    Divide,

    /// <summary>
    /// Применяет логическое И к двум числам на стеке вычислений.
    /// </summary>
    And,

    /// <summary>
    /// Применяет логическое ИЛИ к двум числам на стеке вычислений.
    /// </summary>
    Or,

    /// <summary>
    /// Применяет логическое НЕ к одному числу на стеке вычислений.
    /// </summary>
    Not,

    /// <summary>
    /// Останавливает выполнение программы.
    /// </summary>
    Halt,
}