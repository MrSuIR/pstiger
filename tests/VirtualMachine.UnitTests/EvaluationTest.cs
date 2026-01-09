using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Instructions;

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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
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
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },

            // Сравнение чисел на равенство
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.Push, 7),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.Push, 7),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.NotEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.Push, 7),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.Push, 7),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.NotEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },

            // Сравнение строк на равенство
            {
                // ("Hello" = "Hello") = 1
                [
                    new Instruction(InstructionCode.Push, "Hello"),
                    new Instruction(InstructionCode.Push, "Hello"),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // ("Hello" <> "Hello") = 0
                [
                    new Instruction(InstructionCode.Push, "Hello"),
                    new Instruction(InstructionCode.Push, "Hello"),
                    new Instruction(InstructionCode.NotEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // ("Hello" = "Bye") = 1
                [
                    new Instruction(InstructionCode.Push, "Hello"),
                    new Instruction(InstructionCode.Push, "Bye"),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // ("Hello" <> "Bye") = 1
                [
                    new Instruction(InstructionCode.Push, "Hello"),
                    new Instruction(InstructionCode.Push, "Bye"),
                    new Instruction(InstructionCode.NotEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },

            // Сравнение чисел на "меньше" и "меньше или равно"
            {
                // (17 < 20) = 1
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // (17 < 17) = 0
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // (17 < 14) = 0
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 14),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // (17 <= 20) = 1
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // (17 <= 17) = 1
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // (17 <= 14) = 0
                [
                    new Instruction(InstructionCode.Push, 17),
                    new Instruction(InstructionCode.Push, 14),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },

            // Сравнение строк на "меньше" и "меньше или равно"
            {
                // ("abc" < "abc") = 0
                [
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // ("abc" <= "abc") = 1
                [
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // ("abc" < "abd") = 1
                [
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Push, "abd"),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // ("abc" <= "abd") = 1
                [
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Push, "abd"),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(1)
            },
            {
                // ("abd" < "abc") = 0
                [
                    new Instruction(InstructionCode.Push, "abd"),
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // ("abd" <= "abc") = 0
                [
                    new Instruction(InstructionCode.Push, "abd"),
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },
            {
                // ("abc" <= "ABC") = 0
                [
                    new Instruction(InstructionCode.Push, "abc"),
                    new Instruction(InstructionCode.Push, "ABC"),
                    new Instruction(InstructionCode.LessOrEqual),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(0)
            },

            // Унарный минус в арифметическом выражении
            {
                [
                    new Instruction(InstructionCode.Push, 1024),
                    new Instruction(InstructionCode.Negate),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(-1024)
            },

            // Удаление значения с вершины стека
            {
                [
                    new Instruction(InstructionCode.Push, 1024),
                    new Instruction(InstructionCode.Push, 702),
                    new Instruction(InstructionCode.Pop),
                    new Instruction(InstructionCode.Negate),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(-1024)
            },

            // Дублирование значения на вершине стека
            {
                [
                    new Instruction(InstructionCode.Push, 2),
                    new Instruction(InstructionCode.Duplicate),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.StoreResult),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                new Value(4)
            },
        };
    }
}