using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Builtins;
using PsTiger.VirtualMachine.Instructions;

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
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, 11),
                    new Instruction(InstructionCode.Call, 7),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Функция square(x: int): int
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.DefineVar, "x"),
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
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, "Hello, world!"),
                    new Instruction(InstructionCode.Call, 6),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Начало функции printLine
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.DefineVar, "text"),
                    new Instruction(InstructionCode.LoadVar, "text"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.Push, "\n"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "Hello, world!\n"
            },

            // Можно вызвать функцию для обработки строки с параметрами
            {
                // function concat3(a: str, b: str, c: str): str = concat(a, concat(b, c))
                // print(concat3("po", "ta", "to"))
                [
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, "po"),
                    new Instruction(InstructionCode.Push, "ta"),
                    new Instruction(InstructionCode.Push, "to"),
                    new Instruction(InstructionCode.Call, 9),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Начало функции concat3
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.DefineVar, "c"),
                    new Instruction(InstructionCode.DefineVar, "b"),
                    new Instruction(InstructionCode.DefineVar, "a"),
                    new Instruction(InstructionCode.LoadVar, "a"),
                    new Instruction(InstructionCode.LoadVar, "b"),
                    new Instruction(InstructionCode.LoadVar, "c"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Concat),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Concat),
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
                        function square(x: int): int = (
                            x := x * x;
                            x
                        )
                    in
                        printi(square(x));
                        print(", ");
                        printi(x)
                    end
                 */ [
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.DefineVar, "x"),

                    // printi(square(x));
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Call, 13),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    // print(", ");
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),

                    // printi(x)
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Начало функции function square(x: int): int
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.DefineVar, "x"),

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

            // Функция захватывает переменные внешней области видимости
            // Функция создаёт новую область видимости
            {
                /*
                   Код ниже эквивалентен следующей программе:
                    let
                        var x : int := 10
                        function squareX(): int = x * x
                    in
                        let
                            var x : int := 20
                        in
                            printi(squareX());
                            print(", ");
                            printi(x)
                        end
                    end
                 */ [
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.DefineVar, "x"),

                    // let
                    //    var x : int := 20
                    // in
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.DefineVar, "x"),

                    // printi(squareX())
                    new Instruction(InstructionCode.Call, 16),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),

                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Начало функции function squareX(): int
                    new Instruction(InstructionCode.PushVars, 1),

                    // x * x
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Multiply),

                    // Конец функции function squareX(): int
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "100, 20"
            },
        };
    }
}