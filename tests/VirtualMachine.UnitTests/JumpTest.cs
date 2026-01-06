using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class JumpTest
{
    [Theory]
    [MemberData(nameof(GetJumpOverInstructionsData))]
    public void Can_jump_over_instructions(
        List<Instruction> program,
        string expectedBufferedOutput
    )
    {
        FakeEnvironment environment = new();
        TigerVm vm = new(environment, program);
        Value result = vm.RunProgram();

        Assert.Equal(0, vm.ExitCode);
        Assert.Equal(Value.Void, result);
        Assert.Equal(expectedBufferedOutput, environment.BufferedOutput);
        Assert.Equal(string.Empty, environment.FlushedOutput);
    }

    public static TheoryData<List<Instruction>, string> GetJumpOverInstructionsData()
    {
        return new TheoryData<List<Instruction>, string>
        {
            // Безусловный переход
            {
                [
                    new Instruction(InstructionCode.Jump, 4),
                    new Instruction(InstructionCode.Push, "Should not be printed"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 1),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.Push, 8),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "18"
            },

            // Условный переход, если на вершине стека — ненулевое значение
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.NotEqual),
                    new Instruction(InstructionCode.JumpIfTrue, 7),
                    new Instruction(InstructionCode.Push, "Should not be printed"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 1),
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "68"
            },
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.JumpIfTrue, 7),
                    new Instruction(InstructionCode.Push, "Should be printed"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 0),
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "Should be printed"
            },

            // Условный переход, если на вершине стека — нулевое значение
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.NotEqual),
                    new Instruction(InstructionCode.JumpIfFalse, 7),
                    new Instruction(InstructionCode.Push, "Should be printed"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 0),
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "68"
            },
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.JumpIfFalse, 7),
                    new Instruction(InstructionCode.Push, "Should not be printed"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 0),
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "Should be printed"
            },
        };
    }
}