using Grammar;

using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class IfElseTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateIfElseData))]
    public void Can_evaluate_if_else(string code, Value expectedResult, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Value result = interpreter.Execute(code);
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, Value, string> GetEvaluateIfElseData()
    {
        return new TheoryData<string, Value, string>
        {
            // Ветвление с веткой then без else
            {
                """
                if 2 * 2 = 4
                then
                    print("yes")
                """,
                Value.Void, "yes"
            },
            {
                """
                if 2 * 2 = 5
                then
                    print("yes")
                """,
                Value.Void, ""
            },

            // Ветвление с ветками then и else без возвращаемого значения
            {
                """
                if 2 * 2 = 4
                then
                    print("yes")
                else
                    print("no")
                """,
                Value.Void, "yes"
            },
            {
                """
                if 2 * 2 = 5
                then
                    print("yes")
                else
                    print("no")
                """,
                Value.Void, "no"
            },

            // Ветвление с ветками then и else с возвращаемым значением
            {
                """
                if 7 - 1 = 6
                then
                    "yes"
                else
                    "no"
                """,
                new Value("yes"), ""
            },

            // Нет проблемы висячего else
            {
                """
                if 2 * 2 = 4
                then
                    if 7 - 1 = 5
                    then
                        print("yes")
                    else
                        print("no")
                """,
                Value.Void, "no"
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
            // Условие должно быть целочисленным выражением
            {
                """
                if "yes"
                then
                    print("yes")
                else
                    print("no")
                """,
                typeof(TypeErrorException)
            },

            // Ветка then в отсутствие else не может возвращать значение
            {
                """
                if 0
                then
                    "yes"
                """,
                typeof(TypeErrorException)
            },

            // Ветки then и else должны возвращать значения одного типа данных
            {
                """
                if 0
                then
                    "yes"
                else
                    0
                """,
                typeof(TypeErrorException)
            },
            {
                """
                if 0
                then
                    "yes"
                else
                    print("no")
                """,
                typeof(TypeErrorException)
            },
        };
    }
}