using PsTiger.Ast.Expressions;
using PsTiger.Parsing;
using PsTiger.Semantics;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary;

using Reqnroll;

using Xunit;

using Parser = PsTiger.Parsing.Parser;
using TokenType = PsTiger.Lexemes.TokenType;

namespace PsTiger.Tests.Frontend.Specs.Steps;

[Binding]
public class FrontendStepDefinitions
{
    private string _sourceCode = string.Empty;
    private Expression? _programAst;
    private Exception? _lastException;

    [Given(@"я загрузил программу ""(.*)""")]
    public void ПустьЯЗагрузилПрограмму(string program)
    {
        _sourceCode = Samples.GetSampleProgram(program);
    }

    [When("выполняется синтаксический разбор")]
    public void КогдаВыполняетсяСинтаксическийРазбор()
    {
        try
        {
            Parser parser = new(_sourceCode);
            _programAst = parser.ParseProgram();
        }
        catch (Exception e)
        {
            _lastException = e;
        }
    }

    [When("успешно выполняется синтаксический разбор")]
    public void КогдаУспешноВыполняетсяСинтаксическийРазбор()
    {
        Parser parser = new(_sourceCode);
        _programAst = parser.ParseProgram();
    }

    [When("выполняется семантический анализ")]
    public void КогдаВыполняетсяСемантическийАнализ()
    {
        Assert.NotNull(_programAst);
        try
        {
            SemanticsChecker checker = new();
            checker.Check(_programAst);
        }
        catch (Exception e)
        {
            _lastException = e;
        }
    }

    [Then(@"возникнет неожиданная лексема ""(.*)""")]
    public void ТогдаВозникнетНеожиданнаяЛексема(string tokenTypeString)
    {
        UnexpectedLexemeException e = Assert.IsType<UnexpectedLexemeException>(_lastException);
        TokenType tokenType = Enum.Parse<TokenType>(tokenTypeString);
        Assert.Equal(tokenType, e.Actual);
    }

    [Then(@"возникнет ошибка неправильного присваивания")]
    public void ТогдаВозникнетОшибкаНеправильногоПрисваивания()
    {
        Assert.IsType<InvalidAssignmentException>(_lastException);
    }

    [Then(@"возникнет ошибка из-за неизвестного символа ""(.*)""")]
    public void ТогдаВозникнетОшибкаИзЗаНеизвестногоСимвола(string name)
    {
        UnknownSymbolException e = Assert.IsType<UnknownSymbolException>(_lastException);
        Assert.Equal(name, e.Name);
    }

    [Then(@"возникнет ошибка из-за недопустимого символа ""(.*)""")]
    public void ТогдаВозникнетОшибкаИзЗаНедопустимогоСимвола(string name)
    {
        InvalidSymbolException e = Assert.IsType<InvalidSymbolException>(_lastException);
        Assert.Equal(name, e.Name);
    }

    [Then(@"возникнет ошибка из-за дублирующего символа ""(.*)""")]
    public void ТогдаВозникнетОшибкаИзЗаДублирующегоСимвола(string name)
    {
        DuplicateSymbolException e = Assert.IsType<DuplicateSymbolException>(_lastException);
        Assert.Equal(name, e.Name);
    }

    [Then(@"возникнет ошибка неправильного литерала структуры")]
    public void ТогдаВозникнетОшибкаНеправильногоЛитералаСтруктуры()
    {
        Assert.IsType<InvalidRecordLiteralException>(_lastException);
    }

    [Then(@"возникнет ошибка типизации")]
    public void ТогдаВозникнетОшибкаТипизации()
    {
        Assert.IsType<TypeErrorException>(_lastException);
    }

    [Then(@"возникнет ошибка из‑за недопустимого вызова функции")]
    public void ТогдаВозникнетОшибкаИзЗаНедопустимогоВызоваФункции()
    {
        Assert.IsType<InvalidFunctionCallException>(_lastException);
    }

    [Then(@"возникнет ошибка из‑за недопустимого выражения")]
    public void ТогдаВозникнетОшибкаИзЗаНедопустимогоВыражения()
    {
        Assert.IsType<InvalidExpressionException>(_lastException);
    }
}