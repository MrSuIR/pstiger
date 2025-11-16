namespace PsTiger.Lexemes.UnitTests;

public class LexerTest
{
    [Theory]
    [MemberData(nameof(GetTokenizeIdentifiersAndKeywordsData))]
    [MemberData(nameof(GetTokenizeLiteralsData))]
    [MemberData(nameof(GetTokenizePunctuationData))]
    public void Can_tokenize_lexemes(string code, List<Token> expected)
    {
        List<Token> actual = Tokenize(code);
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, List<Token>> GetTokenizeIdentifiersAndKeywordsData()
    {
        return new TheoryData<string, List<Token>>
        {
            {
                "alice Bob C00L D_2 x", [
                    new Token(TokenType.Identifier, "alice"),
                    new Token(TokenType.Identifier, "Bob"),
                    new Token(TokenType.Identifier, "C00L"),
                    new Token(TokenType.Identifier, "D_2"),
                    new Token(TokenType.Identifier, "x")
                ]
            },
            {
                "type  arrtype = array of int", [
                    new Token(TokenType.Type),
                    new Token(TokenType.Identifier, "arrtype"),
                    new Token(TokenType.Equal),
                    new Token(TokenType.Array),
                    new Token(TokenType.Of),
                    new Token(TokenType.Identifier, "int")
                ]
            },
            {
                "let var x := 0 in x end", [
                    new Token(TokenType.Let),
                    new Token(TokenType.Var),
                    new Token(TokenType.Identifier, "x"),
                    new Token(TokenType.Assign),
                    new Token(TokenType.Literal, 0),
                    new Token(TokenType.In),
                    new Token(TokenType.Identifier, "x"),
                    new Token(TokenType.End),
                ]
            },
            {
                "while isdigit(buffer) do break", [
                    new Token(TokenType.While),
                    new Token(TokenType.Identifier, "isdigit"),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, "buffer"),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Do),
                    new Token(TokenType.Break),
                ]
            },
            {
                "if n = 0 then 1 else n * factorial(n-1)", [
                    new Token(TokenType.If),
                    new Token(TokenType.Identifier, "n"),
                    new Token(TokenType.Equal),
                    new Token(TokenType.Literal, 0),
                    new Token(TokenType.Then),
                    new Token(TokenType.Literal, 1),
                    new Token(TokenType.Else),
                    new Token(TokenType.Identifier, "n"),
                    new Token(TokenType.Multiply),
                    new Token(TokenType.Identifier, "factorial"),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, "n"),
                    new Token(TokenType.Minus),
                    new Token(TokenType.Literal, 1),
                    new Token(TokenType.CloseParenthesis),
                ]
            },
            {
                "for i:=10 to 20 do printi(i)", [
                    new Token(TokenType.For),
                    new Token(TokenType.Identifier, "i"),
                    new Token(TokenType.Assign),
                    new Token(TokenType.Literal, 10),
                    new Token(TokenType.To),
                    new Token(TokenType.Literal, 20),
                    new Token(TokenType.Do),
                    new Token(TokenType.Identifier, "printi"),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, "i"),
                    new Token(TokenType.CloseParenthesis),
                ]
            },
            {
                "function foo() = ()", [
                    new Token(TokenType.Function),
                    new Token(TokenType.Identifier, "foo"),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Equal),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.CloseParenthesis),
                ]
            },
            {
                "if a=nil then b", [
                    new Token(TokenType.If),
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.Equal),
                    new Token(TokenType.Nil),
                    new Token(TokenType.Then),
                    new Token(TokenType.Identifier, "b"),
                ]
            },
        };
    }

    public static TheoryData<string, List<Token>> GetTokenizeLiteralsData()
    {
        return new TheoryData<string, List<Token>>
        {
            {
                "0 1234 56789 0017", [
                    new Token(TokenType.Literal, 0),
                    new Token(TokenType.Literal, 1234),
                    new Token(TokenType.Literal, 56789),
                    new Token(TokenType.Literal, 17),
                ]
            },
            {
                """
                "" "0" "Hello, world!"
                """,
                [
                    new Token(TokenType.Literal, ""),
                    new Token(TokenType.Literal, "0"),
                    new Token(TokenType.Literal, "Hello, world!"),
                ]
            },
            {
                """
                "\n\t\"\\"
                """,
                [
                    new Token(TokenType.Literal, "\n\t\"\\"),
                ]
            },
        };
    }

    public static TheoryData<string, List<Token>> GetTokenizePunctuationData()
    {
        return new TheoryData<string, List<Token>>
        {
            {
                "x + y / (10 - z * 2)", [
                    new Token(TokenType.Identifier, "x"),
                    new Token(TokenType.Plus),
                    new Token(TokenType.Identifier, "y"),
                    new Token(TokenType.Divide),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Literal, 10),
                    new Token(TokenType.Minus),
                    new Token(TokenType.Identifier, "z"),
                    new Token(TokenType.Multiply),
                    new Token(TokenType.Literal, 2),
                    new Token(TokenType.CloseParenthesis),
                ]
            },
            {
                "a < b | a > c | b = c", [
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.LessThan),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.Or),
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.GreaterThan),
                    new Token(TokenType.Identifier, "c"),
                    new Token(TokenType.Or),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.Equal),
                    new Token(TokenType.Identifier, "c"),
                ]
            },
            {
                "a <= b & b >= c & a <> c", [
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.LessThanOrEqual),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.And),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.GreaterThanOrEqual),
                    new Token(TokenType.Identifier, "c"),
                    new Token(TokenType.And),
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.NotEqual),
                    new Token(TokenType.Identifier, "c"),
                ]
            },
            {
                "speed.x := v[0]", [
                    new Token(TokenType.Identifier, "speed"),
                    new Token(TokenType.Dot),
                    new Token(TokenType.Identifier, "x"),
                    new Token(TokenType.Assign),
                    new Token(TokenType.Identifier, "v"),
                    new Token(TokenType.OpenBracket),
                    new Token(TokenType.Literal, 0),
                    new Token(TokenType.CloseBracket),
                ]
            },
            {
                "type list = {first: int, rest: list}", [
                    new Token(TokenType.Type),
                    new Token(TokenType.Identifier, "list"),
                    new Token(TokenType.Equal),
                    new Token(TokenType.OpenBrace),
                    new Token(TokenType.Identifier, "first"),
                    new Token(TokenType.Colon),
                    new Token(TokenType.Identifier, "int"),
                    new Token(TokenType.Comma),
                    new Token(TokenType.Identifier, "rest"),
                    new Token(TokenType.Colon),
                    new Token(TokenType.Identifier, "list"),
                    new Token(TokenType.CloseBrace),
                ]
            },
            {
                "(foo(x);0)", [
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, "foo"),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, "x"),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.Literal, 0),
                    new Token(TokenType.CloseParenthesis),
                ]
            },
        };
    }

    private static List<Token> Tokenize(string code)
    {
        List<Token> results = [];
        Lexer lexer = new(code);
        for (Token t = lexer.ParseToken(); t.Type != TokenType.EndOfFile; t = lexer.ParseToken())
        {
            results.Add(t);
        }

        return results;
    }
}