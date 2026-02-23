using Grammar;

using PsTiger.Interpreter;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class LoopTest
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

            // Область видимости итератора цикла ограничена самим циклом
            {
                """
                (
                    for i := 0 to 2 do printi(i);
                    printi(i)
                )
                """,
                typeof(UnknownSymbolException)
            },

            // Инструкция break не допускается за пределами циклов
            {
                """
                (
                    for i := 0 to 2 do printi(i);
                    break
                )
                """,
                typeof(InvalidExpressionException)
            },

            // Инструкция break не допускается, если цикл объявлен в одной из вызывающих функций и отсутствует в вызванной
            {
                """
                let
                    var stopLoop: int := 0
                in
                    while not(stopLoop) do (
                        let
                            function breakInFunction() = break
                        in
                            for x := 1 to 100 do (
                                if x * x > 20
                                then
                                    breakInFunction();
                                printi(x);
                                print(" ")
                            )
                        end;
                        stopLoop := 1
                    )
                end
                """,
                typeof(InvalidExpressionException)
            },
        };
    }
}