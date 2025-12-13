using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;

namespace Interpreter.IntegrationTests;

public class TypeDeclarationsTest
{
    [Theory]
    [MemberData(nameof(GetDeclareTypeAliasData))]
    public void Can_declare_type_alias(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    [Theory]
    [MemberData(nameof(GetDeclareArrayTypeData))]
    public void Can_declare_array_type(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string> GetDeclareTypeAliasData()
    {
        return new TheoryData<string, string>
        {
            // Можно объявить синоним встроенного типа
            {
                """
                let
                    type intToo = int
                    type stringToo = string
                    var number: intToo := 10
                    var text: stringToo := "hello"
                in
                    printi(number);
                    print(" ");
                    print(text)
                end
                """,
                "10 hello"
            },

            // Можно объявить тип, перекрыв имя встроенного типа
            {
                """
                let
                    type tmp = string
                    type string = int
                    type int = tmp
                    var number: string := 10
                    var text: int := "hello"
                in
                    printi(number);
                    print(" ");
                    print(text)
                end
                """,
                "10 hello"
            },

            // Можно использовать одно имя для типа и для переменной/функции в одной области видимости
            {
                """
                let
                    type text = string
                    var int: int := 10
                    var text: text := "hello"
                in
                    printi(int);
                    print(" ");
                    print(text)
                end
                """,
                "10 hello"
            },
        };
    }

    public static TheoryData<string, string> GetDeclareArrayTypeData()
    {
        return new TheoryData<string, string>
        {
            // Можно объявить тип одномерного массива строк
            {
                """
                let
                    type text = array of string
                in
                end
                """,
                ""
            },

            // Можно объявить тип двумерного массива целых чисел
            {
                """
                let
                    type row = array of int
                    type table = array of row
                in
                end
                """,
                ""
            },
        };
    }
}