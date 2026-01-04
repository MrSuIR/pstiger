namespace PsTiger.VirtualMachine;

public enum InstructionCode
{
    /// <summary>
    /// Добавляет значение в стек значений.
    /// </summary>
    Push,

    /// <summary>
    /// Останавливает выполнение программы.
    /// </summary>
    Halt,
}