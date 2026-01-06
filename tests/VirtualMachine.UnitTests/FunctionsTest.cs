using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class FunctionsTest
{
    [Theory]
    [MemberData(nameof(GetCallUserDefinedFunctionsData))]
    public void Can_call_user_defined_functions(
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

    public static TheoryData<List<Instruction>, string> GetCallUserDefinedFunctionsData()
    {
        return new TheoryData<List<Instruction>, string>
        {
            // Можно вызвать функцию для вычисления числа с параметрами
            {
                // function square(x: int): int = x * x
                // printi(square(11))
                [
                    new Instruction(InstructionCode.Push, 11),
                    new Instruction(InstructionCode.Call, 4),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                    new Instruction(InstructionCode.StoreVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.Return),
                ],
                "121"
            },
        };
    }
}