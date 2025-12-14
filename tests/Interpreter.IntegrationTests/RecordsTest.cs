using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;

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

            // Присваивание переменных с типом структуры создаёт ссылку на неё, а не копию
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p1 : Point := Point{x = 10, y = 20}
                    var p2 := p1
                    var p3 : Point := Point{x = 10, y = 20}
                    function printPoint(p: Point) = (
                        printi(p.x);
                        print(", ");
                        printi(p.y)
                    )
                in
                    p1.x := 33;
                    printPoint(p1);
                    print("; ");
                    printPoint(p2);
                    print("; ");
                    printPoint(p3)
                end
                """,
                "33, 20; 33, 20; 10, 20"
            },

            // Структура передаётся в функцию по ссылке, а не по значению
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p1 : Point := Point{x = 10, y = 20}
                    function movePoint(p: Point, dx: int, dy: int) = (
                       p.x := p.x + dx;
                       p.y := p.y + dy
                    )
                    function printPoint(p: Point) = (
                        printi(p.x);
                        print(", ");
                        printi(p.y)
                    )
                in
                    movePoint(p1, -5, 17);
                    printPoint(p1)
                end
                """,
                "5, 37"
            },

            // Структура существует после завершения области видимости, в которой была создана
            {
                """
                let
                    type Point = { x: int, y: int }
                    function printPoint(p: Point) = (
                        printi(p.x);
                        print(", ");
                        printi(p.y)
                    )
                    var point := Point{ x = 0, y = 0 }
                in
                    let
                        function makePoint(x: int, y: int): Point = Point{ x = x, y = y }
                        var temp : Point := makePoint(-7, 14)
                    in
                        point := temp
                    end;
                    printPoint(point)
                end
                """,
                "-7, 14"
            },

            // Сравнение структур сравнивает их по ссылке, а не по значениям полей
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p1 : Point := Point{x = 10, y = 20}
                    var p2 : Point := Point{x = 10, y = 20}
                in
                    printi(p1 = p2);
                    printi(p2 = p1);
                    printi(p1 = p1);
                    printi(p2 = p2);
                    print(" ");
                    printi(p1 <> p2);
                    printi(p2 <> p1);
                    printi(p1 <> p1);
                    printi(p2 <> p2)
                end
                """,
                "0011 1100"
            },

            // Можно сравнивать структуры разных типов, если эти типы являются синонимами
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p1 : Point := Point{x = 10, y = 20}
                    type Vector = Point
                    var p2 : Vector := Vector{x = 10, y = 20}
                in
                    printi(p1 = p2);
                    printi(p1 <> p2)
                end
                """,
                "01"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidRecordUsageData))]
    public void Rejects_invalid_record_usage(string code, Type exceptionType)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Assert.Throws(exceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidRecordUsageData()
    {
        return new TheoryData<string, Type>
        {
            // В объявлении структуры нельзя объявлять поля с одинаковым именем
            {
                """
                let
                    type Point = { x: int, x: int }
                    var p : Point := Point{x = 10, x = 20}
                in
                    printi(p.x)
                end
                """,
                typeof(DuplicateSymbolException)
            },

            // В литерале структуры нельзя инициализировать поля, которых нет в объявлении
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p : Point := Point{x = 10, y = 20, z = 10}
                in
                    printi(p.x)
                end
                """,
                typeof(InvalidRecordLiteralException)
            },

            // В литерале структуры нельзя инициализировать поле значением другого типа
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p : Point := Point{x = 10, y = "0" }
                in
                    printi(p.x)
                end
                """,
                typeof(TypeErrorException)
            },

            // В литерале структуры нельзя инициализировать поля в другом порядке
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p : Point := Point{y = 20, x = 10}
                in
                    printi(p.x)
                end
                """,
                typeof(InvalidRecordLiteralException)
            },

            // В литерале структуры нельзя пропускать инициализацию полей
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p : Point := Point{x = 10}
                in
                    printi(p.x)
                end
                """,
                typeof(InvalidRecordLiteralException)
            },

            // Для структур не действует оператор сравнения `<`
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p1 : Point := Point{x = 10, y = 20}
                    var p2 : Point := Point{x = 10, y = 20}
                in
                    printi(p1 < p2)
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя сравнивать структуры разных типов, если эти типы не являются синонимами
            {
                """
                let
                    type Point = { x: int, y: int }
                    type Vector = { x: int, y: int }
                    var p1 : Point := Point{x = 10, y = 20}
                    var p2 : Vector := Vector{x = 10, y = 20}
                in
                    printi(p1 = p2)
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя записать поле структуры, если его нет в объявлении типа структуры
            {
                """
                let
                    type Point = { x: int, y: int }
                    var p : Point := Point{x = 10, y = 20}
                in
                    p.z := 10
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя запрашивать поле у переменной, не являющейся структурой
            {
                """
                let
                    type Point = array of int
                    var p : Point := Point[2] of 0
                in
                    p.x := 10
                end
                """,
                typeof(TypeErrorException)
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetUseRecursiveTypesData))]
    public void Can_use_recursive_types(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string> GetUseRecursiveTypesData()
    {
        return new TheoryData<string, string>
        {
            // Можно объявить рекурсивную структуру (односвязный список) и использовать её
            {
                """
                let
                    type ListNode = { value: int, next: ListNode }
                    function printList(node: ListNode) = (
                        printi(node.value);
                        if node.next <> nil then (
                            print(", ");
                            printList(node.next)
                        )
                    )
                    var list := ListNode{
                        value = 5,
                        next = ListNode{
                            value = 17,
                            next = nil
                        }
                    }
                in
                    printList(list)
                end
                """,
                "5, 17"
            },

            // Можно объявить и использовать взаимно рекурсивные структуры
            {
                """
                let
                    type A = { value: int, next: B }
                    type B = { value: int, next: A }
                    function printA(a: A) = (
                        print("a=");
                        printi(a.value);
                        if a.next <> nil then (
                            print(", ");
                            printB(a.next)
                        )
                    )
                    function printB(b: B) = (
                        print("b=");
                        printi(b.value);
                        if b.next <> nil then (
                            print(", ");
                            printA(b.next)
                        )
                    )
                    var a := A{
                        value = 5,
                        next = B{
                            value = 12,
                            next = A{
                               value = 8,
                               next = nil
                            }
                        }
                    }
                in
                    printA(a)
                end
                """,
                "a=5, b=12, a=8"
            },
        };
    }
}