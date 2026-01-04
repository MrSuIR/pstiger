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
        TigerVm vm = new(environment, instructions);
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

            // Сложение и вычитание с помощью стека
            {
                [
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.Push, 50),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.Push, 3),
                    new Instruction(InstructionCode.Subtract),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(67)
            },

            // Умножение и деление с помощью стека
            {
                [
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.Push, 50),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.Push, -5),
                    new Instruction(InstructionCode.Divide),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(-200)
            },

            // Вычисление логических выражений
            {
                // 1 & 0 | 1 = 1
                [
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.And),
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Or),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(1)
            },
            {
                // 1 & (0 | 1) = 1
                [
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Or),
                    new Instruction(InstructionCode.And),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(1)
            },
            {
                // 1 & (0 & 1) = 0
                [
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.And),
                    new Instruction(InstructionCode.And),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(0)
            },
            {
                // 1 & not(0) = 1
                [
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Not),
                    new Instruction(InstructionCode.And),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(1)
            },
            {
                // 1 & not(1) = 0
                [
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Not),
                    new Instruction(InstructionCode.And),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                new Value(0)
            },
        };
    }
}