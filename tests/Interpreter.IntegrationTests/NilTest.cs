using Grammar;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class NilTest
{
    [Theory]
    [MemberData(nameof(GetUseNilWithRecordsData))]
    public void Can_use_nil_with_records(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string> GetUseNilWithRecordsData()
    {
        return new TheoryData<string, string>
        {
            // Можно инициализировать переменную значением nil с указанием типа структуры `var a: record := nil`
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        printi(p.x);
                        print(", ");
                        printi(p.y)
                    )
                    var point: Point := nil
                in
                    point := Point{ x = 10, y = 12 };
                    printPoint(point)
                end
                """,
                "10, 12"
            },

            // Можно присваивать значение nil переменной структуры
            // Можно проверять на равенство ссылки на структуру и nil
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        if p <> nil then (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        ) else (
                            print("nil")
                        )
                    )
                    var point: Point := Point{ x = 10, y = 12 }
                in
                    point := nil;
                    printPoint(point)
                end
                """,
                "nil"
            },

            // Можно передавать nil как аргумент функции, ожидающей структуру
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        if p <> nil then (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        ) else (
                            print("nil")
                        )
                    )
                in
                    printPoint(nil)
                end
                """,
                "nil"
            },

            // Можно использовать nil для инициализации поля структуры
            {
                """
                let
                    type Point = { x: int, y: int }
                    type Triangle = { a: Point, b: Point, c: Point }
                    function printPoint(p: Point) = (
                        if p <> nil then (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        ) else (
                            print("nil")
                        )
                    )
                    var t: Triangle := Triangle{
                        a = Point{x = 0, y = 0},
                        b = Point{x = 3, y = 0},
                        c = nil
                    }
                in
                    printPoint(t.a);
                    print("; ");
                    printPoint(t.b);
                    print("; ");
                    printPoint(t.c)
                end
                """,
                "0, 0; 3, 0; nil"
            },

            // Можно вернуть nil в ветке then конструкции if...then...else
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        if p <> nil then (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        ) else (
                            print("nil")
                        )
                    )
                    var enabled: int := 1
                    var point: Point := if not(enabled) then nil else Point{x = 0, y = 0}
                in
                    printPoint(point)
                end
                """,
                "0, 0"
            },

            // Можно вернуть nil в ветке else конструкции if...then...else
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        if p <> nil then (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        ) else (
                            print("nil")
                        )
                    )
                    var enabled: int := 1
                    var point: Point := if enabled then Point{x = 0, y = 0} else nil
                in
                    printPoint(point)
                end
                """,
                "0, 0"
            },

            // Можно вернуть nil в обоих ветках конструкции if...then...else
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        if p <> nil then (
                            printi(p.x);
                            print(", ");
                            printi(p.y)
                        ) else (
                            print("nil")
                        )
                    )
                    var enabled: int := 1
                    var point: Point := if enabled then nil else nil
                in
                    printPoint(point)
                end
                """,
                "nil"
            },
        };
    }

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