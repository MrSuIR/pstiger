using PsTiger.Interpreter;
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
        Assert.Equal(result, expected, EqualityComparer<Value>.Default);
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
        };
    }
}