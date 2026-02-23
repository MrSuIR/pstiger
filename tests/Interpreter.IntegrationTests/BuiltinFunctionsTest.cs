using Grammar;

using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Exceptions;

namespace Interpreter.IntegrationTests;

public class BuiltinFunctionsTest
{
    [Fact]
    public void Stops_on_invalid_char_conversion()
    {
        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Assert.Throws<ProgramAbortedException>(() => interpreter.Execute("chr(2025)"));
    }

    [Theory]
    [MemberData(nameof(GetInvalidFunctionCallsData))]
    public void Throws_on_invalid_function_calls(string code, Type expectedExceptionType)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Assert.Throws(expectedExceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidFunctionCallsData()
    {
        return new TheoryData<string, Type>
        {
            // Нельзя вызвать неизвестную функцию
            {
                "length(\"Hello!\")", typeof(UnknownSymbolException)
            },

            // Нельзя вызвать встроенную функцию с неправильными типами аргументов
            {
                "size(10)", typeof(TypeErrorException)
            },

            // Нельзя вызвать встроенную функцию с неправильным числом аргументов
            {
                "size(\"Hello!\", \"World\")", typeof(InvalidFunctionCallException)
            },
            {
                "size()", typeof(InvalidFunctionCallException)
            },
            {
                "concat()", typeof(InvalidFunctionCallException)
            },
            {
                "concat(\"a\")", typeof(InvalidFunctionCallException)
            },
            {
                "concat(\"a\", \"b\", \"c\")", typeof(InvalidFunctionCallException)
            },
        };
    }
}