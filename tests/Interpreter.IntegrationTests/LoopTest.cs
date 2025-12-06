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

            // Цикл for выполняется с шагом 1 от нижней до верхней границы (включительно)
            {
                """
                for i := 5 to 7 do (
                    printi(i);
                    print(" ")
                )
                """,
                Value.Void, "5 6 7 "
            },

            // Цикл for выполняется однократно, если нижняя и верхняя границы равны
            {
                """
                for i := 3 to 3 do (
                    printi(i);
                    print(" ")
                )
                """,
                Value.Void, "3 "
            },

            // Цикл for не выполняется ни разу, если нижняя граница больше верхней
            {
                """
                for i := 3 to 2 do (
                    printi(i);
                    print(" ")
                )
                """,
                Value.Void, ""
            },
        };
    }
}