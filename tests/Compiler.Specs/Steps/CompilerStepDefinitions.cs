using System.Text;

using PsTiger.Tests.Compiler.Specs.Drivers;
using PsTiger.Tests.TestLibrary;
using PsTiger.Tests.TestLibrary.Helpers;

using Reqnroll;

using Xunit;

namespace PsTiger.Tests.Compiler.Specs.Steps;

[Binding]
public sealed class CompilerStepDefinitions : IDisposable
{
    private const int RunTimeoutSeconds = 20;

    private readonly CompilerTestDriver _compilerTestDriver = new();
    private TempFile? _compiledProgram = null;

    private StringBuilder _programInput = new();
    private string _lastProgramOutput = string.Empty;
    private int _lastProgramExitCode = -1;

    [Given(@"^я скомпилировал программу ""(.*)""$")]
    public void ПустьЯСкомпилировалПрограмму(string relativeProgramPath)
    {
        string programPath = Samples.GetSampleProgramPath(relativeProgramPath);

        Assert.True(File.Exists(programPath), $"Source code file {programPath} does not exist");

        _compiledProgram ??= TempFile.CreateEmpty("program-", ".exe");
        _compilerTestDriver.RunCompiler(programPath, _compiledProgram.Path);
    }

    [When(@"я ввожу (.*)")]
    public void КогдаЯВвожу(string input)
    {
        _programInput.Append(input);
    }

    [When(@"я ввожу текст:")]
    public void КогдаЯВвожуТекст(string input)
    {
        _programInput.Append(input);
    }

    [When("^(?:я )?выполняю программу$")]
    public async Task КогдаВыполняюПрограмму()
    {
        Assert.NotNull(_compiledProgram);
        Assert.True(File.Exists(_compiledProgram.Path), $"Executable file {_compiledProgram.Path} does not exist");

        CancellationTokenSource cts = new(TimeSpan.FromSeconds(RunTimeoutSeconds));
        _lastProgramOutput = await DotnetConsoleProgramRunner.CheckedRunAndReadOutput(
            _compiledProgram.Path, _programInput.ToString(), cts.Token
        );
        _lastProgramExitCode = 0;
    }

    [Then("^(?:я )?увижу вывод (.*)$")]
    public void ТогдаЯУвижуВывод(string expected)
    {
        Assert.Equal(expected, _lastProgramOutput);
    }

    [Then("^(?:я )?увижу вывод:$")]
    public void ТогдаЯУвижуВыводМногострочный(string expected)
    {
        Assert.Equal(expected, _lastProgramOutput);
    }

    [Then(@"^(?:я )?получу код возврата (\d+)$")]
    public void ТогдаЯПолучуКодВозврата(int exitCode)
    {
        Assert.Equal(exitCode, _lastProgramExitCode);
    }

    public void Dispose()
    {
        _compiledProgram?.Dispose();
    }
}