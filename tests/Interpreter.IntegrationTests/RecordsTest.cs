using Grammar;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class RecordsTest
{
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