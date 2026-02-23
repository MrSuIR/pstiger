using Grammar;

using PsTiger.Ast.Expressions;
using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class FunctionsTests
{
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