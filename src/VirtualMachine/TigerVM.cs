using PsTiger.Execution;

namespace PsTiger.VirtualMachine;

public class TigerVM
{
    private readonly IEnvironment _environment;
    private readonly IReadOnlyList<Instruction> _instructions;
    private int _instructionPointer;

    public TigerVM(IEnvironment environment, IReadOnlyList<Instruction> instructions)
    {
        ValidateInstructions(instructions);

        _environment = environment;
        _instructions = instructions;
        _instructionPointer = 0;
    }

    public int RunProgram()
    {
        while (true)
        {
            Instruction instruction = _instructions[_instructionPointer++];
            switch (instruction.Code)
            {
                case InstructionCode.Halt:
                    return instruction.Operand.AsInt();
                default:
                    throw new NotImplementedException($"Unknown instruction: {instruction}");
            }
        }
    }

    private static void ValidateInstructions(IReadOnlyList<Instruction> instructions)
    {
        if (instructions.Count == 0)
        {
            throw new InvalidOperationException("Invalid empty VM program");
        }

        if (instructions[^1].Code != InstructionCode.Halt)
        {
            throw new InvalidOperationException($"Last instruction must be Halt, got {instructions[^1]}");
        }
    }
}