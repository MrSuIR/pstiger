using Grammar;

using PsTiger.Interpreter;
using PsTiger.Tests.TestLibrary;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Exceptions;

using Reqnroll;

using Xunit;

namespace PsTiger.Tests.Interpreter.Specs.Steps;

[Binding]
public sealed class InterpreterStepDefinitions
{
    private string _program = string.Empty;
    private readonly FakeEnvironment _fakeEnvironment;
    private readonly TigerInterpreter _interpreter;
    private Exception? _lastException = null;

    public InterpreterStepDefinitions()
    {
        _fakeEnvironment = new FakeEnvironment();
        _interpreter = new TigerInterpreter(_fakeEnvironment);
    }

    [Given(@"^я загрузил программу ""(.*)""$")]
    public void ПустьЯЗагрузилПрограмму(string program)
    {
        _program = Samples.GetSampleProgram(program);
        TigerGrammar.CheckProgramSyntax(_program);
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
        _interpreter.Execute(_program);
    }

    [When("^(?:я )?я выполняю программу с перехватом исключений$")]
    public void КогдаЯВыполняюПрограммуСПерехватомИсключений()
    {
        try
        {
            _interpreter.Execute(_program);
        }
        catch (Exception e)
        {
            _lastException = e;
        }
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
        Assert.Equal(exitCode, _interpreter.ExitCode);
    }

    [Then("^(?:я )?я получу ошибку времени выполнения с сообщением:$")]
    public void ТогдаЯПолучуОшибкуВремениВыполнения(string message)
    {
        ProgramAbortedException e = Assert.IsType<ProgramAbortedException>(_lastException);
        Assert.Equal(message, e.Message);
    }
}