using PsTiger.Interpreter;
using PsTiger.Runtime;

namespace Interpreter.IntegrationTests;

public class BuiltinFunctionsTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateBuiltinFuntionsData))]
    public void Can_evaluate_builtin_functions(string code, Value expected)
    {
        TigerInterpreter interpreter = new();
        Value result = interpreter.Execute(code);
        Assert.Equal(expected, result, EqualityComparer<Value>.Default);
    }


    public static TheoryData<string, Value> GetEvaluateBuiltinFuntionsData()
    {
        return new TheoryData<string, Value>
        {
            {
                // Логические функции
                "1 & not(0)", new Value(1)
            },
            {
                "0 | not(2)", new Value(0)
            },
        };
    }
}