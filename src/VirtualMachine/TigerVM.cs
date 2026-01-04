using PsTiger.Execution;
using PsTiger.Runtime;

namespace PsTiger.VirtualMachine;

public class TigerVM
{
    private readonly IEnvironment _environment;
    private readonly IReadOnlyList<Instruction> _instructions;

    private int _instructionPointer;
    private int _exitCode;
    private readonly Stack<Value> _evaluationStack;

    public TigerVM(IEnvironment environment, IReadOnlyList<Instruction> instructions)
    {
        ValidateInstructions(instructions);

        _environment = environment;
        _instructions = instructions;
        _instructionPointer = 0;
        _exitCode = 0;
        _evaluationStack = new Stack<Value>();
    }

    public int ExitCode => _exitCode;

    public Value RunProgram()
    {
        while (true)
        {
            Instruction instruction = _instructions[_instructionPointer++];
            switch (instruction.Code)
            {
                case InstructionCode.Halt:
                    _exitCode = instruction.Operand.AsInt();
                    return _evaluationStack.TryPop(out Value? result) ? result : Value.Void;

                case InstructionCode.Push:
                    _evaluationStack.Push(instruction.Operand);
                    break;

                default:
                    throw new NotImplementedException($"Unsupported instruction code: {instruction.Code}");
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