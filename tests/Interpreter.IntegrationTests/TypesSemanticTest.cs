using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;

namespace Interpreter.IntegrationTests;

public class TypesSemanticTest
{
    [Theory]
    [MemberData(nameof(GetExpressionsWithTypeErrorsData))]
    public void Rejects_expressions_with_type_errors(string code)
    {
        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Assert.Throws<TypeErrorException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string> GetExpressionsWithTypeErrorsData()
    {
        // Нельзя проводить арифметические операции со строками
        return
        [
            """
            10 + 4 + "5"
            """,

            """
            "cat" + 0
            """,

            """
            "hot" + " " + "dog"
            """,

            """
            "cat" * 2
            """,

            """
            "ten" / 2
            """,

            """
            "10" - 5
            """,

            """
            -"one"
            """,

            // Нельзя проводить логические операции со строками
            """
            1 & "5"
            """,
            """
            0 & "5"
            """,
            """
            (1 | "cat") | 1
            """,

            // Нельзя сравнивать строки с числами
            """
            10 < "11"
            """,
            """
            "one" > 2
            """,
            """
            10 <= "11"
            """,
            """
            "one" >= 2
            """,
            """
            "infinity" = 19
            """,
            """
            "infinity" <> 10
            """,

            // Нельзя проводить арифметические и логические операции с операндом, не возвращающим значения
            """
            10 + ()
            """,
            """
            () * 2
            """,
            """
            () & ()
            """,
            """
            1 | ()
            """,
            """
            1 | (2; ())
            """,
        ];
    }
}