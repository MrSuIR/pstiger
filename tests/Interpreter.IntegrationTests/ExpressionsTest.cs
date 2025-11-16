using PsTiger.Interpreter;

using Runtime;

namespace Interpreter.IntegrationTests;

public class ExpressionsTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionsData))]
    public void Can_evaluate_expressions(string code, Value expected)
    {
        TigerInterpreter interpreter = new();
        Value result = interpreter.Execute(code);
        Assert.Equal(result, expected);
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
        };
    }
}