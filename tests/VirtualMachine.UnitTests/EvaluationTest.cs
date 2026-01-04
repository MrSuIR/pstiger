using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class EvaluationTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionData))]
    public void Can_evaluate_expression(List<Instruction> instructions, Value expected)
    {
        FakeEnvironment environment = new();
        TigerVM vm = new(environment, instructions);
        Value result = vm.RunProgram();

        Assert.Equal(0, vm.ExitCode);
        Assert.Equal(expected, result);
        Assert.Empty(environment.BufferedOutput);
        Assert.Empty(environment.FlushedOutput);
    }

    public static TheoryData<List<Instruction>, Value> GetEvaluateExpressionData()
    {
        return new TheoryData<List<Instruction>, Value>
        {
            // Возврат одного значения со стека
            {
                [
                    new Instruction(InstructionCode.Push, 67),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(67)
            },
        };
    }
}