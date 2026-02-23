using Grammar;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class NilTest
{
    [Theory]
    [MemberData(nameof(GetInvalidNilUsageData))]
    public void Rejects_invalid_nil_usage(string code, Type exceptionType)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Assert.Throws(exceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidNilUsageData()
    {
        return new TheoryData<string, Type>
        {
            // Нельзя инициализировать переменную значением nil без указания типа структуры `var a := nil`
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        printi(p.x);
                        print(", ");
                        printi(p.y)
                    )
                    var point := nil
                in
                    point := Point{ x = 10, y = 12 };
                    printPoint(point)
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя сравнивать на равенство два значения nil: `if nil = nil then ...`
            {
                """
                printi(nil = nil)
                """,
                typeof(TypeErrorException)
            },
        };
    }
}