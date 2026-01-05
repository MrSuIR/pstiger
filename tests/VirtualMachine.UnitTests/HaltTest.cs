using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class HaltTest
{
    [Theory]
    [MemberData(nameof(GetHaltVmData))]
    public void Can_halt_VM(int exitCode)
    {
        FakeEnvironment environment = new();
        TigerVm vm = new(environment, [
            new Instruction(InstructionCode.Halt, exitCode),
        ]);
        Value result = vm.RunProgram();

        Assert.Equal(exitCode, vm.ExitCode);
        Assert.Equal(Value.Void, result);
        Assert.Empty(environment.BufferedOutput);
        Assert.Empty(environment.FlushedOutput);
    }

    public static TheoryData<int> GetHaltVmData()
    {
        return
        [
            0, // Остановка виртуальной машины с нулевым кодом
            1, // Остановка виртуальной машины с ненулевым кодом
        ];
    }
}