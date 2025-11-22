using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Execution;
using PsTiger.Interpreter;
using PsTiger.Runtime;

namespace Interpreter.IntegrationTests;

public class BuiltinFunctionsTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateBuiltinFuntionsData))]
    public void Can_evaluate_builtin_functions(string code, Value expected)
    {
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

    public static TheoryData<string, Value> GetEvaluateBuiltinFuntionsData()
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
        };
    }

    [Theory]
    [MemberData(nameof(GetEvaluateOutputFunctionsData))]
    public void Can_evaluate_output_functions(string code, string expectedBufferedOutput, string expectedFlushedOutput)
    {
        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Value result = interpreter.Execute(code);

        Assert.Equal(result, new Value());
        Assert.Equal(expectedBufferedOutput, environment.BufferedOutput);
        Assert.Equal(expectedFlushedOutput, environment.FlushedOutput);
    }

    public static TheoryData<string, string, string> GetEvaluateOutputFunctionsData()
    {
        // Функции вывода
        return new TheoryData<string, string, string>
        {
            {
                "print(\"Hello!\")", "Hello!", ""
            },
            {
                "printi(2 + 7)", "9", ""
            },
            {
                "(printi(2 + 7); print(\"\\n\"); flush(); printi(2 - 7); print(\"\\n\"))", "-5\n", "9\n"
            },
            {
                "(printi(7); flush(); printi(4))", "4", "7"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetEvaluateInputFunctionsData))]
    public void Can_evaluate_input_functions(string code, string input, string expectedBufferedOutput)
    {
        FakeEnvironment environment = new();
        environment.AddInput(input);

        TigerInterpreter interpreter = new(environment);
        Value result = interpreter.Execute(code);

        Assert.Equal(result, new Value());
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
}