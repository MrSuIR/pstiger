using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Builtins;
using PsTiger.VirtualMachine.Instructions;

namespace PsTiger.VirtualMachine.UnitTests;

public class RecordsTest
{
    [Theory]
    [MemberData(nameof(GetUseRecordsAndNilData))]
    public void Can_use_records_and_nil(
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

    public static TheoryData<List<Instruction>, string> GetUseRecordsAndNilData()
    {
        return new TheoryData<List<Instruction>, string>
        {
            // Создание структуры, запись и чтение её полей
            {
                /*
                 Код ниже эквивалентен следующей программе:
                    let
                        type Point = { x: int, y: int }
                        function printPoint(p: Point) = (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        )
                        var point : Point := Point{x = 10, y = 20}
                    in
                        point.x = point.x * 3;
                        printPoint(point)
                    end
                 */ [
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, Value.NewRecord()),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.InitField, "x"),
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.InitField, "y"),
                    new Instruction(InstructionCode.DefineVar, "point"),

                    // p.x = p.x * 3;
                    new Instruction(InstructionCode.LoadVar, "point"),
                    new Instruction(InstructionCode.LoadField, "x"),
                    new Instruction(InstructionCode.Push, 3),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.LoadVar, "point"),
                    new Instruction(InstructionCode.StoreField, "x"),

                    // printPoint(p);
                    new Instruction(InstructionCode.LoadVar, "point"),
                    new Instruction(InstructionCode.Call, 18),

                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Начало функции printPoint(p: Point).
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.DefineVar, "p"),

                    // printi(p.x):
                    new Instruction(InstructionCode.LoadVar, "p"),
                    new Instruction(InstructionCode.LoadField, "x"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    // print(", "):
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),

                    // printi(p.y):
                    new Instruction(InstructionCode.LoadVar, "p"),
                    new Instruction(InstructionCode.LoadField, "y"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    // Конец функции printPoint(p: Point).
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),
                ],
                "30, 20"
            },

            // Инициализация переменной структуры значением nil
            {
                /*
                 Код ниже эквивалентен следующей программе:
                    let
                        type Point = { x: int, y: int }
                        function printPoint(p: Point) = (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        )
                        var point: Point := nil
                    in
                        point := Point{ x = 10, y = 12 };
                        printPoint(point)
                    end
                 */ [
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, Value.Nil),
                    new Instruction(InstructionCode.DefineVar, "point"),

                    // point := Point{ x = 10, y = 12 };
                    new Instruction(InstructionCode.Push, Value.NewRecord()),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.InitField, "x"),
                    new Instruction(InstructionCode.Push, 12),
                    new Instruction(InstructionCode.InitField, "y"),
                    new Instruction(InstructionCode.StoreVar, "point"),

                    // printPoint(p);
                    new Instruction(InstructionCode.LoadVar, "point"),
                    new Instruction(InstructionCode.Call, 14),

                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),

                    // Начало функции printPoint(p: Point).
                    new Instruction(InstructionCode.PushVars, 1),
                    new Instruction(InstructionCode.DefineVar, "p"),

                    // printi(p.x):
                    new Instruction(InstructionCode.LoadVar, "p"),
                    new Instruction(InstructionCode.LoadField, "x"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    // print(", "):
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.Print),

                    // printi(p.y):
                    new Instruction(InstructionCode.LoadVar, "p"),
                    new Instruction(InstructionCode.LoadField, "y"),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Return),

                    // Конец функции printPoint(p: Point).
                ],
                "10, 12"
            },

            // Сравнение структуры с nil
            {
                /*
                 Код ниже эквивалентен следующей программе:
                    let
                        type Point = { x: int, y: int }
                        var point: Point := nil
                    in
                        printi(point = nil)
                    end
                 */ [
                    new Instruction(InstructionCode.PushVars, 0),
                    new Instruction(InstructionCode.Push, Value.Nil),
                    new Instruction(InstructionCode.DefineVar, "point"),

                    // printi(point = nil)
                    new Instruction(InstructionCode.LoadVar, "point"),
                    new Instruction(InstructionCode.Push, Value.Nil),
                    new Instruction(InstructionCode.Equal),
                    new Instruction(InstructionCode.CallBuiltin, (int)BuiltinFunctionCode.PrintI),

                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.Halt),
                ],
                "1"
            },
        };
    }
}