using PsTiger.Tests.Compiler.Specs.Drivers;
using PsTiger.Tests.TestLibrary;
using PsTiger.Tests.TestLibrary.Helpers;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Exceptions;

using Reqnroll;

using Xunit;

namespace PsTiger.Tests.Compiler.Specs.Steps;

[Binding]
public sealed class CompilerStepDefinitions : IDisposable
{
    private readonly FakeEnvironment _fakeEnvironment;
    private readonly CompilerTestDriver _compilerTestDriver;
    private int _lastExitCode = -1;
    private TempFile? _compiledProgram = null;

    public CompilerStepDefinitions()
    {
        _fakeEnvironment = new FakeEnvironment();
        _compilerTestDriver = new CompilerTestDriver();
    }

    [Given(@"^я скомпилировал программу ""(.*)""$")]
    public void ПустьЯСкомпилировалПрограмму(string programPath)
    {
        Assert.True(File.Exists(programPath), $"Source code file {programPath} does not exist");

        _compiledProgram ??= TempFile.CreateEmpty("program-", "exe");
        _compilerTestDriver.RunCompiler(programPath, _compiledProgram.Path);
    }

    [When(@"я ввожу (.*)")]
    public void КогдаЯВвожу(string input)
    {
        _fakeEnvironment.AddInput(input);
    }

    [When(@"я ввожу текст:")]
    public void КогдаЯВвожуТекст(string input)
    {
        _fakeEnvironment.AddInput(input);
    }

    [When("^(?:я )?выполняю программу$")]
    public void КогдаВыполняюПрограмму()
    {
        throw new PendingStepException();
    }

    [Then("^(?:я )?увижу вывод (.*)$")]
    public void ТогдаЯУвижуВывод(string expected)
    {
        Assert.Equal(expected, _fakeEnvironment.BufferedOutput + _fakeEnvironment.FlushedOutput);
    }

    [Then("^(?:я )?увижу вывод:$")]
    public void ТогдаЯУвижуВыводМногострочный(string expected)
    {
        Assert.Equal(expected, _fakeEnvironment.BufferedOutput + _fakeEnvironment.FlushedOutput);
    }

    [Then(@"^(?:я )?получу код возврата (\d+)$")]
    public void ТогдаЯПолучуКодВозврата(int exitCode)
    {
        Assert.Equal(exitCode, _lastExitCode);
    }

    public void Dispose()
    {
        _compiledProgram?.Dispose();
    }
}