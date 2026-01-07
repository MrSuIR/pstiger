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

                    // Функция square(x: int)
                    new Instruction(InstructionCode.PushVars),
                    new Instruction(InstructionCode.StoreVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "121"
            },

            // Можно вызвать процедуру с параметрами
            {
                // function printLine(text: str) = (print(text); print("\n"))
                // printLine("Hello, world!")
                [
                    new Instruction(InstructionCode.Push, "Hello, world!"),
                    new Instruction(InstructionCode.Call, 3),
                    new Instruction(InstructionCode.Halt, 0),

                    // Начало функции printLine
                    new Instruction(InstructionCode.PushVars),
                    new Instruction(InstructionCode.StoreVar, "text"),
                    new Instruction(InstructionCode.LoadVar, "text"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Push, "\n"),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "Hello, world!\n"
            },

            // Можно вызвать функцию для обработки строки с параметрами
            {
                // function concat3(a: str, b: str, c: str) = concat(a, concat(b, c))
                // print(concat3("po", "ta", "to"))
                [
                    new Instruction(InstructionCode.Push, "po"),
                    new Instruction(InstructionCode.Push, "ta"),
                    new Instruction(InstructionCode.Push, "to"),
                    new Instruction(InstructionCode.Call, 6),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.Halt, 0),

                    // Начало функции concat3
                    new Instruction(InstructionCode.PushVars),
                    new Instruction(InstructionCode.StoreVar, "c"),
                    new Instruction(InstructionCode.StoreVar, "b"),
                    new Instruction(InstructionCode.StoreVar, "a"),
                    new Instruction(InstructionCode.LoadVar, "a"),
                    new Instruction(InstructionCode.LoadVar, "b"),
                    new Instruction(InstructionCode.LoadVar, "c"),
                    new Instruction(InstructionCode.CallBuiltin, "concat"),
                    new Instruction(InstructionCode.CallBuiltin, "concat"),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "potato"
            },

            // Функция создаёт новую область видимости
            {
                /*
                   Код ниже эквивалентен следующей программе:
                    let
                        var x : int := 10
                        function square(x: int) = (
                            x := x * x;
                            x
                        )
                    in
                        printi(square(x));
                        print(", ");
                        printi(x)
                    end
                 */ [
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.StoreVar, "x"),

                    // printi(square(x));
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Call, 10),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    // print(", ");
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, "print"),

                    // printi(x)
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    new Instruction(InstructionCode.Halt, 0),

                    // Начало функции function square(x: int)
                    new Instruction(InstructionCode.PushVars),
                    new Instruction(InstructionCode.StoreVar, "x"),

                    // x := x * x;
                    // x
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.StoreVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),

                    // Конец функции function square(x: int)
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "100, 10"
            },
        };
    }
}