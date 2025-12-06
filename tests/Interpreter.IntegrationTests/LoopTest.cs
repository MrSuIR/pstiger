using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;

namespace Interpreter.IntegrationTests;

public class LoopTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateLoopData))]
    public void Can_evaluate_loop(string code, Value expectedResult, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Value result = interpreter.Execute(code);
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, Value, string> GetEvaluateLoopData()
    {
        return new TheoryData<string, Value, string>
        {
            // Цикл while не выполняется, если условие изначально ложно
            {
                """
                while 0 do
                    print("yes")
                """,
                Value.Void, ""
            },

            // Цикл while выполняется до тех пор, пока условие истинно
            {
                """
                let
                    var n := 5
                in
                    while n > 2 do (
                        printi(n);
                        print(" ");
                        n := n - 1
                    )
                end
                """,
                Value.Void, "5 4 3 "
            },

            // Цикл for выполняется с шагом 1 от нижней до верхней границы (включительно)
            {
                """
                for i := 5 to 7 do (
                    printi(i);
                    print(" ")
                )
                """,
                Value.Void, "5 6 7 "
            },

            // Цикл for выполняется однократно, если нижняя и верхняя границы равны
            {
                """
                for i := 3 to 3 do (
                    printi(i);
                    print(" ")
                )
                """,
                Value.Void, "3 "
            },

            // Цикл for не выполняется ни разу, если нижняя граница больше верхней
            {
                """
                for i := 3 to 2 do (
                    printi(i);
                    print(" ")
                )
                """,
                Value.Void, ""
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
            // Цикл while нельзя использовать там, где ожидается возврат значения
            {
                """
                let
                    var x := 0
                in
                    x := while x > 0 do (
                        printi(x);
                        x := x - 1
                    )
                end
                """,
                typeof(TypeErrorException)
            },

            // Выражение условия в цикле while может возвращать только целочисленный тип
            {
                """
                let
                    var str := "true"
                in
                    while str do (
                        print(str);
                        str := "false"
                    )
                end
                """,
                typeof(TypeErrorException)
            },

            // Тело цикла while не может возвращать значения
            {
                """
                let
                    var x := 0
                in
                    while x > 0 do (
                        printi(x);
                        x := x - 1;
                        x
                    )
                end
                """,
                typeof(TypeErrorException)
            },

            // Первые два выражения в цикле for могут возвращать только целочисленный тип
            {
                """
                for i := "zero" to 2 do printi(i)
                """,
                typeof(TypeErrorException)
            },
            {
                """
                for i := 0 to "hero" do printi(i)
                """,
                typeof(TypeErrorException)
            },
            {
                """
                for i := "zero" to "hero" do printi(i)
                """,
                typeof(TypeErrorException)
            },

            // Итератору цикла for нельзя присвоить значение
            {
                """
                for i := 0 to 2 do (
                    printi(i);
                    i := i + 1
                )
                """,
                typeof(InvalidAssignmentException)
            },

            // Цикл for нельзя использовать там, где ожидается возврат значения
            {
                """
                let
                    var x := 0
                in
                    x := for i := 0 to 2 do printi(i)
                end
                """,
                typeof(TypeErrorException)
            },

            // Тело цикла for не может возвращать значения
            {
                """
                for i := 0 to 2 do i
                """,
                typeof(TypeErrorException)
            },
        };
    }
}