using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class CallBuiltinTest
{
    [Theory]
    [MemberData(nameof(GetUseInputAndOutputData))]
    public void CanUseInputAndOutput(
        List<Instruction> program,
        string input,
        string expectedBufferedOutput,
        string expectedFlushedOutput
    )
    {
        FakeEnvironment environment = new();
        environment.AddInput(input);

        TigerVm vm = new(environment, program);
        Value result = vm.RunProgram();

        Assert.Equal(0, vm.ExitCode);
        Assert.Equal(Value.Void, result);
        Assert.Equal(expectedBufferedOutput, environment.BufferedOutput);
        Assert.Equal(expectedFlushedOutput, environment.FlushedOutput);
    }

    public static TheoryData<List<Instruction>, string, string, string> GetUseInputAndOutputData()
    {
        return new TheoryData<List<Instruction>, string, string, string>()
        {
            // Функция print
            {
                [
                    new Instruction(InstructionCode.Push, "Hello, world!"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                string.Empty, "Hello, world!", string.Empty
            },

            // Функция printi
            {
                [
                    new Instruction(InstructionCode.Push, 762),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                string.Empty, "762", string.Empty
            },

            // Функция flush
            {
                [
                    new Instruction(InstructionCode.Push, "Hello, world!"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Push, 111),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.CallBuiltin, "flush"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                string.Empty, string.Empty, "Hello, world!111"
            },

            // Функция getchar
            {
                [
                    new Instruction(InstructionCode.CallBuiltin, "getchar"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.CallBuiltin, "getchar"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "Input", "In", string.Empty
            },
        };
    }
}