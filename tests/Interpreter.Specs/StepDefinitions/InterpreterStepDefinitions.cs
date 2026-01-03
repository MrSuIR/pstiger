using PsTiger.Interpreter;
using PsTiger.Tests.TestLibrary;
using PsTiger.Tests.TestLibrary.TestDoubles;

using Reqnroll;

using Xunit;

namespace PsTiger.Tests.Interpreter.Specs.StepDefinitions;

[Binding]
public sealed class InterpreterStepDefinitions
{
    private string _program = string.Empty;
    private readonly FakeEnvironment _fakeEnvironment;
    private readonly TigerInterpreter _interpreter;

    public InterpreterStepDefinitions()
    {
        _fakeEnvironment = new FakeEnvironment();
        _interpreter = new TigerInterpreter(_fakeEnvironment);
    }

    [Given(@"я загрузил программу (.*)")]
    public void ПустьЯЗагрузилПрограмму(string program)
    {
        _program = Samples.GetSampleProgram(program);
    }

    [When(@"я ввожу (.*)")]
    public void КогдаЯВвожу(string input)
    {
        _fakeEnvironment.AddInput(input);
    }

    [When(@"выполняю программу")]
    public void КогдаВыполняюПрограмму()
    {
        _interpreter.Execute(_program);
    }

    [Then(@"я увижу вывод (.*)")]
    public void ТогдаЯУвижуВывод(string expected)
    {
        Assert.Equal(expected, _fakeEnvironment.BufferedOutput + _fakeEnvironment.FlushedOutput);
    }
}