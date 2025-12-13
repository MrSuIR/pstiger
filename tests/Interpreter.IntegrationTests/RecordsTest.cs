using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;

namespace Interpreter.IntegrationTests;

public class RecordsTest
{
    [Theory]
    [MemberData(nameof(GetUseRecordTypeData))]
    public void Can_use_record_type(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string> GetUseRecordTypeData()
    {
        return new TheoryData<string, string>
        {
            // Можно создать структуру Point(x, y), присвоить и прочитать её поля
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p : Point := Point{x = 10, y = 20}
                in
                    printi(p.x);
                    print(", ");
                    printi(p.y)
                end
                """,
                "10, 20"
            },
        };
    }
}