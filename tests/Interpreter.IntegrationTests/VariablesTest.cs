using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;
using PsTiger.Parsing;
using PsTiger.Semantics.Exceptions;

namespace Interpreter.IntegrationTests;

public class VariablesTest
{
    [Fact]
    public void Can_calculate_rectangle_square()
    {
        const string code =
            """
            let
              var x1 := 0
              var y1 := 4
              var x2 := 6
              var y2 := 7
              var width: int := 0
              var height: int := 0
              var square: int := 0
            in
              width := x2 - x1;
              height := y2 - y1;
              square := width * height;
              printi(square)
            end
            """;
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("18", environment.BufferedOutput);
    }

    [Fact]
    public void Can_print_hello_world()
    {
        const string code =
            """
            let
              var greeting: string := ""
              var exclamation := "!"
              var space := " "
            in
              greeting := concat("Hello", space);
              greeting := concat(greeting, "world");
              greeting := concat(greeting, exclamation);
              print(greeting)
            end
            """;
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("Hello world!", environment.BufferedOutput);
    }

    [Fact]
    public void Allows_to_shadow_builtin_function_with_variable()
    {
        const string code =
            """
            let
              var print: int := 10
            in
              printi(print)
            end
            """;
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("10", environment.BufferedOutput);
    }

    // Допускается `let ... in end` без выражения между `in` и `end`
    [Fact]
    public void Allows_scope_without_expression()
    {
        const string code =
            """
            let
              var x: int := 10
            in
            end
            """;
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("", environment.BufferedOutput);
    }

    // Конструкция `let ... in ... end` возвращает результат последнего выражения
    [Fact]
    public void Let_in_returns_last_expression_result()
    {
        const string code =
            """
            printi(
              let
                var x: int := 10
              in
                x + x;
                x * x
              end
            )
            """;

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("100", environment.BufferedOutput);
    }

    [Theory]
    [MemberData(nameof(GetSyntaxViolationsData))]
    public void Throws_on_syntax_violations(string code, Type expectedExceptionType)
    {
        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Assert.Throws(expectedExceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetSyntaxViolationsData()
    {
        return new TheoryData<string, Type>
        {
            // Присваивание не возвращает результата: `a := b := 0` недопустимо
            {
                """
                let
                    var x: int := 0,
                    var y: int := 0
                in
                   x := y := 0
                end
                """,
                typeof(UnexpectedLexemeException)
            },

            // Выражение слева в присваивании должно быть переменной
            {
                """
                let
                  var x : int := 10
                in
                  10 := x;
                  printi(x)
                end
                """,
                typeof(InvalidAssignmentException)
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetSemanticViolationsData))]
    public void Throws_on_semantic_violations(string code, Type expectedExceptionType)
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
            // Нельзя использовать необъявленную переменную
            {
                "x", typeof(UnknownSymbolException)
            },
            {
                "print(x)", typeof(UnknownSymbolException)
            },

            // Нельзя инициализировать переменную значением другого типа
            {
                """
                let
                  var x : int := "Hello"
                in
                  printi(x)
                end
                """,
                typeof(TypeErrorException)
            },
            {
                """
                let
                  var x : string := 10
                in
                  printi(x)
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя использовать переменную, объявленную в другой области видимости
            {
                """
                (
                    let
                      var x : int := 10
                    in
                      printi(x)
                    end;
                    let
                      var y : int := 20
                    in
                      printi(x)
                    end
                )
                """,
                typeof(UnknownSymbolException)
            },

            // Нельзя вызывать переменную как функцию
            {
                """
                let
                  var x : int := 10
                in
                  printi(x())
                end
                """,
                typeof(InvalidSymbolException)
            },

            // Нельзя вызывать встроенную функцию, если её имя перекрыто переменной
            {
                """
                let
                  var printi : int := 10
                in
                  printi(10)
                end
                """,
                typeof(InvalidSymbolException)
            },

            // Нельзя присвоить переменной значение другого типа
            {
                """
                let
                  var x : int := 10
                in
                  x := "eleven";
                  printi(x)
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя повторно объявлять переменную с тем же именем в одной области видимости
            {
                """
                let
                    var x : int := 10
                    var x : int := 12
                in
                    printi(x)
                end
                """,
                typeof(DuplicateSymbolException)
            },
        };
    }
}