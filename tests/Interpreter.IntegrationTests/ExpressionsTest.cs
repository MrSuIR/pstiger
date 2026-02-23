using System.Diagnostics;

using Grammar;

using PsTiger.Interpreter;
using PsTiger.Parsing;
using PsTiger.Runtime;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class ExpressionsTest
{
    [Theory]
    [MemberData(nameof(GetInvalidExpressionsData))]
    public void Rejects_invalid_expressions(string code)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
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