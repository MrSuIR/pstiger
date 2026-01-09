using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Builtins;
using PsTiger.VirtualMachine.Instructions;

namespace PsTiger.VirtualMachine.UnitTests;

public class CallBuiltinTest
{
    [Theory]
    [MemberData(nameof(GetUseInputAndOutputData))]
    public void Can_use_input_and_output(
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
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                string.Empty, "Hello, world!", string.Empty
            },

            // Функция printi
            {
                [
                    new Instruction(InstructionCode.Push, 762),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                string.Empty, "762", string.Empty
            },

            // Функция flush
            {
                [
                    new Instruction(InstructionCode.Push, "Hello, world!"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 111),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Flush),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                string.Empty, string.Empty, "Hello, world!111"
            },

            // Функция getchar
            {
                [
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.GetChar),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.GetChar),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "Input", "In", string.Empty
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetCallBuiltinFunctionsData))]
    public void Can_call_builtin_functions(
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

    public static TheoryData<List<Instruction>, string> GetCallBuiltinFunctionsData()
    {
        return new TheoryData<List<Instruction>, string>()
        {
            // Функция ord: ord("ABCD") = ord("A") = 65
            {
                [
                    new Instruction(InstructionCode.Push, "ABCD"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Ord),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "65"
            },

            // Функция chr: chr(40) = "("; chr(41) = ")"
            {
                [
                    new Instruction(InstructionCode.Push, 40),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Chr),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 41),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Chr),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "()"
            },

            // Функция size: size("Hello, world!") = 13
            {
                [
                    new Instruction(InstructionCode.Push, "Hello, world!"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Size),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "13"
            },

            // Функция substring: substring("Cogito, ergo sum", 0, 6) = "Cogito"
            {
                [
                    new Instruction(InstructionCode.Push, "Cogito, ergo sum"),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Push, 6),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Substring),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "Cogito"
            },

            // Функция concat: concat("Cogito", " ergo sum") = "Cogito ergo sum"
            {
                [
                    new Instruction(InstructionCode.Push, "Cogito"),
                    new Instruction(InstructionCode.Push, " ergo sum"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Concat),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "Cogito ergo sum"
            },
        };
    }
}