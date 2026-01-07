using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class ArrayTest
{
    [Theory]
    [MemberData(nameof(GetUseArraysData))]
    public void Can_use_arrays(
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

    public static TheoryData<List<Instruction>, string> GetUseArraysData()
    {
        return new TheoryData<List<Instruction>, string>
        {
            // Инициализация массива, запись и чтение элементов массива
            {
                /*
                 Код ниже эквивалентен следующей программе:
                 let
                   type IntArray = array of int
                   var x := intArray[4] of 7
                 in
                   x[2] := 3
                   printi(x[0])
                   for i := 1 to 4 do (
                     print(", ");
                     printi(x[i])
                   )
                 end
                 */ [
                    new Instruction(InstructionCode.PushVars),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Push, 7),
                    new Instruction(InstructionCode.CreateArray),
                    new Instruction(InstructionCode.DefineVar, "x"),

                    // x[2] := 3
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Push, 2),
                    new Instruction(InstructionCode.Push, 3),
                    new Instruction(InstructionCode.StoreArray),

                    // printi(x[0])
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Push, 0),
                    new Instruction(InstructionCode.LoadArray),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    // Инициализация итератора цикла for: i := 1
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.DefineVar, "i"),

                    // Проверка условия цикла for и переход на инструкцию после цикла, если условие не выполняется.
                    new Instruction(InstructionCode.LoadVar, "i"),
                    new Instruction(InstructionCode.Push, 4),
                    new Instruction(InstructionCode.Less),
                    new Instruction(InstructionCode.JumpIfFalse, 30),

                    // Тело цикла: print(", "); printi(x[i])
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, "print"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "i"),
                    new Instruction(InstructionCode.LoadArray),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    // Инкремент итератора цикла.
                    new Instruction(InstructionCode.LoadVar, "i"),
                    new Instruction(InstructionCode.Push, 1),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.StoreVar, "i"),

                    // Переход к условию цикла for.
                    new Instruction(InstructionCode.Jump, 15),

                    // Конец программы.
                    new Instruction(InstructionCode.PopVars),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "7, 7, 3, 7"
            },
        };
    }
}