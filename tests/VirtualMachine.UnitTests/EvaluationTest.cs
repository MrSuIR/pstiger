using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.VirtualMachine.UnitTests;

public class EvaluationTest
{
    [Theory]
    [MemberData(nameof(GetHaltVmData))]
    public void Can_halt_VM(int exitCode)
    {
        FakeEnvironment environment = new();
        TigerVM vm = new(environment, [
            new Instruction(InstructionCode.Halt, exitCode),
        ]);
        int actualExitCode = vm.RunProgram();

        Assert.Equal(exitCode, actualExitCode);
        Assert.Empty(environment.BufferedOutput);
        Assert.Empty(environment.FlushedOutput);
    }

    public static TheoryData<int> GetHaltVmData()
    {
        return
        [
            0,
            1,
        ];
    }
}