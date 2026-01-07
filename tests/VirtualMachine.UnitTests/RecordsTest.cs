using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

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
            // Создание структуры, запись и чтение полей структуры
            {
                /*
                 Код ниже эквивалентен следующей программе:
                    let
                        type Point = { x: int, y: int }
                        var p : Point := Point{x = 10, y = 20}
                    in
                        printi(p.x);
                        print(", ");
                        printi(p.y)
                    end
                 */ [
                    new Instruction(InstructionCode.Push, Value.NewRecord()),
                    new Instruction(InstructionCode.Push, 10),
                    new Instruction(InstructionCode.InitField, "x"),
                    new Instruction(InstructionCode.Push, 20),
                    new Instruction(InstructionCode.InitField, "y"),
                    new Instruction(InstructionCode.StoreVar, "p"),

                    // printi(p.x):
                    new Instruction(InstructionCode.LoadVar, "p"),
                    new Instruction(InstructionCode.LoadField, "x"),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),

                    // print(", "):
                    new Instruction(InstructionCode.Push, ", "),
                    new Instruction(InstructionCode.CallBuiltin, "print"),

                    // printi(p.y):
                    new Instruction(InstructionCode.LoadVar, "p"),
                    new Instruction(InstructionCode.LoadField, "y"),
                    new Instruction(InstructionCode.CallBuiltin, "printi"),
                ],
                "10, 20"
            },
        };
    }
}