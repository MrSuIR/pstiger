namespace PsTiger.Lexemes.UnitTests;

public class LexerTest
{
    [Theory]
    [MemberData(nameof(GetTokenizerIdentifiersAndKeywordsData))]
    public void Can_tokenize_lexemes(string code, List<Token> expected)
    {
        List<Token> actual = Tokenize(code);
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, List<Token>> GetTokenizerIdentifiersAndKeywordsData()
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
        };
    }

    private List<Token> Tokenize(string code)
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