using PsTiger.Interpreter;
using PsTiger.Parsing;
using PsTiger.Runtime;

namespace Interpreter.IntegrationTests;

public class ExpressionsTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionsData))]
    public void Can_evaluate_expressions(string code, Value expected)
    {
        TigerInterpreter interpreter = new();
        Value result = interpreter.Execute(code);
        Assert.Equal(expected, result, EqualityComparer<Value>.Default);
    }

    [Theory]
    [MemberData(nameof(GetInvalidExpressionsData))]
    public void Rejects_invalid_expressions(string code)
    {
        TigerInterpreter interpreter = new();
        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, Value> GetEvaluateExpressionsData()
    {
        return new TheoryData<string, Value>
        {
            {
                // Разбор арифметических выражений с учётом приоритета
                "1 + 2 * 8 / 3 - 1", new Value(5)
            },
            {
                // Разбор арифметических выражений с учётом скобок
                "(1 + 2) * (8 / (3 - 1))", new Value(12)
            },

            // Проверка левоассоциативности арифметических операций
            {
                "10 - 3 - 2", new Value(5)
            },
            {
                "10 / 3 / 2", new Value(1)
            },
            {
                "10 - 3 + 2", new Value(9)
            },
            {
                "10 / 3 * 2", new Value(6)
            },

            // Разбор унарного минуса
            {
                "-4", new Value(-4)
            },
            {
                "2 * 2 * --5", new Value(20)
            },

            // Разбор операторов сравнения
            {
                "1 + 2 < 5", new Value(1)
            },
            {
                "2 * 2 > 5", new Value(0)
            },
            {
                "2 * 2 = 5", new Value(0)
            },
            {
                "2 / 2 <> 4", new Value(1)
            },
            {
                "2 * 2 >= 4", new Value(1)
            },
            {
                "2 - 1 <= 1", new Value(1)
            },
            {
                "1 = (2 = 3)", new Value(0)
            },

            // Разбор операций сравнения строк
            {
                """
                "Hello" = "Hello!"
                """,
                new Value(0)
            },
            {
                """
                "Hello" <> "Hello!"
                """,
                new Value(1)
            },
            {
                """
                "Bob" > "Alice"
                """,
                new Value(1)
            },
            {
                """
                "Bob" < "Alice"
                """,
                new Value(0)
            },
            {
                """
                "Bob" >= "Alice"
                """,
                new Value(1)
            },
            {
                """
                "Bob" <= "Alice"
                """,
                new Value(0)
            },

            // Разбор логических операторов
            {
                "1 & 0", new Value(0)
            },
            {
                "3 | 2", new Value(1)
            },

            // Разбор последовательности выражений в скобках
            {
                "(2 * 2; 2 * 5)", new Value(10)
            },
        };
    }

    public static TheoryData<string> GetInvalidExpressionsData()
    {
        // Проверка отсутствия ассоциативности сравнений
        return
        [
            "1 < 2 < 3",
            "1 = 2 = 3",
        ];
    }
}