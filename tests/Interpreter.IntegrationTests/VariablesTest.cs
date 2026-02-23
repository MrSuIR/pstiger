using Grammar;

using PsTiger.Interpreter;
using PsTiger.Parsing;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class VariablesTest
{
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

            // Выражение слева в присваивании должно быть lvalue
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