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
    /// Останавливает выполнение программы.
    /// </summary>
    Halt,
}