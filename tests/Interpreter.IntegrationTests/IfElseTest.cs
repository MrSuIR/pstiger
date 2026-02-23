using Grammar;

using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class IfElseTest
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