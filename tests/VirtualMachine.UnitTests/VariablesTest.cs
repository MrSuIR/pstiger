using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class VariablesTest
{
    [Theory]
    [MemberData(nameof(GetStoreAndLoadVariablesData))]
    public void Can_store_and_load_variables(
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

    public static TheoryData<List<Instruction>, string> GetStoreAndLoadVariablesData()
    {
        return new TheoryData<List<Instruction>, string>
        {
            // Сохранение и загрузка переменных
            {
                // (x := 10; y := 14; printi(x * x))
                [
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.StoreVar, "x"),
                    new Instruction(InstructionCode.Push, 14),
                    new Instruction(InstructionCode.StoreVar, "y"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                    new Instruction(InstructionCode.Halt, 0),
                ],
                "100"
            },

            // Переменная внутренней области видимости может перекрыть переменную внешней области видимости
            {
                /*
                   Код ниже эквивалентен следующей программе:
                    let
                        var x : int := 10
                    in
                        let
                            var x : int := 12
                        in
                            printi(x)
                        end;
                        print(", ")
                        printi(x)
                    end
                 */ [
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.StoreVar, "x"),

                    // let
                    //   var x : int := 12
                    // in
                    new Instruction(InstructionCode.PushVars),

                    new Instruction(InstructionCode.Push, 12),
                    new Instruction(InstructionCode.StoreVar, "x"),

                    // printi(x)
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    // end;
                    new Instruction(InstructionCode.PopVars),

                    // print(", ")
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, "print"),

                    // printi(x)
                    new Instruction(InstructionCode.LoadVar, "x"),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    new Instruction(InstructionCode.Halt, 0),
                ],
                "12, 10"
            },
        };
    }
}