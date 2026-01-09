using Grammar;

using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;
using PsTiger.VirtualMachine.Exceptions;

namespace Interpreter.IntegrationTests;

public class BuiltinFunctionsTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateBuiltinFunctionsData))]
    public void Can_evaluate_builtin_functions(string code, Value expected)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Value result = interpreter.Execute(code);
        Assert.Equal(expected, result, EqualityComparer<Value>.Default);
    }

    [Fact]
    public void Stops_on_invalid_char_conversion()
    {
        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Assert.Throws<ProgramAbortedException>(() => interpreter.Execute("chr(2025)"));
    }

    public static TheoryData<string, Value> GetEvaluateBuiltinFunctionsData()
    {
        return new TheoryData<string, Value>
        {
            // Логические функции
            {
                "1 & not(0)", new Value(1)
            },
            {
                "0 | not(2)", new Value(0)
            },

            // Функции преобразования символов
            {
                "chr(ord(\"Hello\"))", new Value("H")
            },
            {
                "ord(\"0\")", new Value(48)
            },
            {
                "chr(49)", new Value("1")
            },
            {
                "ord(\"\")", new Value(-1)
            },

            // Функции работы со строками
            {
                "size(\"Hello!\")", new Value(6)
            },
            {
                "substring(\"Hello!\", 2, 2)", new Value("ll")
            },
            {
                "substring(\"Hello!\", 2, 10)", new Value("llo!")
            },
            {
                "concat(\"Ali\", \"ce\")", new Value("Alice")
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetEvaluateOutputFunctionsData))]
    public void Can_evaluate_output_functions(
        string code,
        Value expectedResult,
        string expectedBufferedOutput,
        string expectedFlushedOutput
    )
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Value result = interpreter.Execute(code);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedBufferedOutput, environment.BufferedOutput);
        Assert.Equal(expectedFlushedOutput, environment.FlushedOutput);
    }

    public static TheoryData<string, Value, string, string> GetEvaluateOutputFunctionsData()
    {
        return new TheoryData<string, Value, string, string>
        {
            // Функции вывода
            {
                "print(\"Hello!\")", Value.Void, "Hello!", ""
            },
            {
                "printi(2 + 7)", Value.Void, "9", ""
            },
            {
                "(printi(2 + 7); print(\"\\n\"); flush(); printi(2 - 7); print(\"\\n\"))", Value.Void, "-5\n", "9\n"
            },
            {
                "(printi(7); flush(); printi(4))", Value.Void, "4", "7"
            },

            // Вычисления логических операций по короткой схеме
            {
                "1 & (printi(2); 0)", new Value(0), "2", ""
            },
            {
                "0 & (printi(0); 0)", new Value(0), "", ""
            },
            {
                "0 | (printi(2); 0)", new Value(0), "2", ""
            },
            {
                "1 | (printi(0); 0)", new Value(1), "", ""
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetEvaluateInputFunctionsData))]
    public void Can_evaluate_input_functions(string code, string input, string expectedBufferedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        environment.AddInput(input);

        TigerInterpreter interpreter = new(environment);
        Value result = interpreter.Execute(code);

        Assert.Equal(result, Value.Void);
        Assert.Equal(expectedBufferedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string, string> GetEvaluateInputFunctionsData()
    {
        return new TheoryData<string, string, string>
        {
            {
                "print(getchar())", "x", "x"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetEvaluateExitFunctionData))]
    public void Can_evaluate_exit_function(string code, string expectedBufferedOutput, int expectedCode)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Value result = interpreter.Execute(code);

        Assert.Equal(result, Value.Void);
        Assert.Equal(expectedCode, interpreter.ExitCode);
        Assert.Equal(expectedBufferedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string, int> GetEvaluateExitFunctionData()
    {
        return new TheoryData<string, string, int>
        {
            {
                "(print(\"Hello, \"); exit(0); print(\"World!\"))", "Hello, ", 0
            },
            {
                "(print(\"The End\"); exit(1))", "The End", 1
            },
        };
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