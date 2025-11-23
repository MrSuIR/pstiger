using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Ast.Expressions;
using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;

namespace Interpreter.IntegrationTests;

public class FunctionsTests
{
    [Theory]
    [MemberData(nameof(GetEvaluateFunctionsData))]
    public void Can_evaluate_if_else(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Value result = interpreter.Execute(code);
        Assert.Equal(Value.Void, result);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string> GetEvaluateFunctionsData()
    {
        return new TheoryData<string, string>
        {
            // Функция без параметров
            {
                """
                let
                    function one(): int = 1
                in
                    printi(one())
                end
                """,
                "1"
            },

            // Функция с одним параметром
            {
                """
                let
                    function abs(x: int): int = if x >= 0 then x else -x
                in
                    printi(abs(7));
                    print(" ");
                    printi(abs(-5));
                print(" ");
                    printi(abs(4))
                end
                """,
                "7 5 4"
            },

            // Функция с несколькими параметрами
            {
                """
                let
                    function rectsquare(x1: int, y1: int, x2: int, y2: int): int =
                        (x2 - x1) * (y2 - y1)
                in
                    printi(rectsquare(1, 3, 6, 7))
                end
                """,
                "20"
            },

            // Процедура без параметров
            {
                """
                let
                    function printHello() = print("Hello, World!")
                in
                    printHello()
                end
                """,
                "Hello, World!"
            },

            // Рекурсивный вызов функции
            {
                """
                let
                    function factorial(x: int): int =
                        if x <= 1 then 1 else x * factorial(x - 1)
                in
                    printi(factorial(5))
                end
                """,
                "120"
            },

            // Взаимная рекурсия функций
            {
                """
                let
                    function f(n: int) = g(n + 1)
                    function g(n: int) = (
                        printi(n);
                        if n < 10 then
                            f(n + 1)
                    )
                in
                    f(1)
                end
                """,
                "246810"
            },

            // Можно скрыть встроенную функцию объявлением пользовательской функции
            {
                """
                let
                    function printi() = print("I")
                in
                    printi()
                end
                """,
                "I"
            },

            // Функция захватывает переменные окружающей области видимости
            {
                """
                let
                    var counter: int := 0
                    function increment() = counter := counter + 1
                    function display() = (printi(counter); print(" "))
                in
                    increment();
                    display();
                    increment();
                    display()
                end
                """,
                "1 2 "
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetSemanticViolationsData))]
    public void Rejects_code_with_semantic_violations(string code, Type expectedExceptionType)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        Assert.Throws(expectedExceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetSemanticViolationsData()
    {
        return new TheoryData<string, Type>
        {
            // Нельзя вызывать процедуру там, где ожидается возврат значения
            {
                """
                let
                    function printHello() = print("Hello, World!")
                    var result: string := ""
                in
                    result := printHello()
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя передавать в функцию неправильное число параметров
            {
                """
                let
                    function printHello() = print("Hello, World!")
                in
                    printHello("World!")
                end
                """,
                typeof(InvalidFunctionCallException)
            },

            // Нельзя передавать в функцию параметры неподходящих типов
            {
                """
                let
                    function printLine(text: string) = print(concat(text, "\n"))
                in
                    printLine(10)
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя использовать в функции переменные, недоступные в её области видимости
            {
                """
                let
                    function printHello() = print(greeting)
                in
                    let
                       var greeting: string := "Hello, World!"
                    in
                        printHello()
                    end
                end
                """,
                typeof(UnknownSymbolException)
            },

            // Нельзя возвращать из функции значение типа, не соответствующего заявленному
            {
                """
                let
                    function printHello() = "Hello, World!"
                in
                    printHello()
                end
                """,
                typeof(TypeErrorException)
            },
        };
    }
}