using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;
using PsTiger.Runtime;

namespace Interpreter.IntegrationTests;

public class LoopTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateLoopData))]
    public void Can_evaluate_loop(string code, Value expectedResult, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Value result = interpreter.Execute(code);
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, Value, string> GetEvaluateLoopData()
    {
        return new TheoryData<string, Value, string>
        {
            // Цикл while не выполняется, если условие изначально ложно
            {
                """
                while 0 do
                    print("yes")
                """,
                Value.Void, ""
            },

            // Цикл while выполняется до тех пор, пока условие истинно
            {
                """
                let
                    var n := 5
                in
                    while n > 2 do (
                        printi(n);
                        print(" ");
                        n := n - 1
                    )
                end
                """,
                Value.Void, "5 4 3 "
            },
        };
    }
}