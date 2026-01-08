using PsTiger.VirtualMachine;

namespace Codegen;

/// <summary>
/// Базовый блок инструкций — это линейная последовательность инструкций виртуальной машины,
///  которая обычно завершается переходом, возвратом либо остановом.
/// </summary>
/// <remarks>
/// В нашем бэкенде, в отличие от LLVM, нет строго правила "базовый блок имеет ровно одну завершающую инструкцию".
/// </remarks>
public class BasicBlock
{
    private readonly int _id;
    private readonly List<Instruction> _instructions = [];

    public BasicBlock(int id)
    {
        _id = id;
    }

    public List<Instruction> Instructions => _instructions;

    /// <summary>
    /// Идентификатор (номер) базового блока.
    /// Используется как промежуточное значение операнда в инструкциях перехода до того,
    ///  как будет создан финальный список инструкций.
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// Добавляет инструкцию в конец базового блока.
    /// </summary>
    public void Append(Instruction instruction)
    {
        _instructions.Add(instruction);
    }
}