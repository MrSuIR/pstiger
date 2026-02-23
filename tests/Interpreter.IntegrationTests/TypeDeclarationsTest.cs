using Grammar;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class TypeDeclarationsTest
{
    [Theory]
    [MemberData(nameof(GetInvalidTypeDeclarationsData))]
    public void Rejects_invalid_type_declarations(string code, Type exceptionType)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Assert.Throws(exceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidTypeDeclarationsData()
    {
        return new TheoryData<string, Type>
        {
            // Нельзя объявить два типа с одинаковым именем
            {
                """
                let
                    type text = string
                    type text = string
                in
                end
                """,
                typeof(DuplicateSymbolException)
            },

            // Нельзя объявить переменную несуществующего типа
            {
                """
                let
                    var t: text := "hello"
                in
                end
                """,
                typeof(UnknownSymbolException)
            },
        };
    }
}