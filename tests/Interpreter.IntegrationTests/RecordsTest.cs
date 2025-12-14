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

            // Можно создать вложенные структуры: Triangle включает Point
            {
                """
                let
                    type Point = { x: int, y: int }
                    type Triangle = { a: Point, b: Point, c: Point }
                    var t: Triangle := Triangle{
                        a = Point{x = 0, y = 0},
                        b = Point{x = 3, y = 0},
                        c = Point{x = 0, y = 4}
                    }
                in
                    printi(t.b.x);
                    print(", ");
                    printi(t.b.y)
                end
                """,
                "3, 0"
            },

            // Можно создать структуру с полями-массивами и работать с ней
            {
                """
                let
                    type IntArrayData = array of int
                    type IntArray = { size: int, data: IntArrayData }
                    function newIntArray(size: int): IntArray = IntArray{
                        size = size,
                        data = IntArrayData[size] of 0
                    }
                    var arr : IntArray := newIntArray(4)
                in
                    for i := 0 to arr.size - 1 do
                        arr.data[i] := (i + 1) * (i + 1);
                
                    printi(arr.data[0]);
                    for i := 1 to arr.size - 1 do (
                        print(", ");
                        printi(arr.data[i])
                    )
                end
                """,
                "1, 4, 9, 16"
            },

            // Можно создать массив структур Point и работать с ним
            {
                """
                let
                    type Point = { x: int, y: int }
                    type PointArray = array of Point
                    var points := PointArray[4] of Point{ x = 0, y = 0 }
                in
                    points[0].x := 1;
                    points[0].y := 2;
                    points[1].x := 3;
                    points[1].y := 4;
                    points[2].x := 5;
                    points[2].y := 6;
                
                    for i := 0 to 3 do (
                        print("{");
                        printi(points[i].x);
                        print(", ");
                        printi(points[i].y);
                        print("} ")
                    )
                end
                """,
                "{1, 2} {3, 4} {5, 6} {0, 0} "
            },

            // Можно создать пустую структуру
            {
                """
                let
                    type NoValueType = {}
                    var noValue := NoValueType{}
                in
                    print("OK")
                end
                """,
                "OK"
            },

            // Можно объявить структуру, поля которой одноимённы переменным в той же области видимости
            {
                """
                let
                    type Point = { x: int, y: int }
                    var x : int := 30
                    var y : int := -16
                    var p : Point := Point{x = x, y = y}
                in
                    printi(p.x);
                    print(", ");
                    printi(p.y)
                end
                """,
                "30, -16"
            },
        };
    }
}