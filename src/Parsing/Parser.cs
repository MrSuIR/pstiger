using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Lexemes;
using PsTiger.Runtime;

using Expression = PsTiger.Ast.Expressions.Expression;

namespace PsTiger.Parsing;

public class Parser
{
    private readonly TokenStream _tokens;

    public Parser(string code)
    {
        _tokens = new TokenStream(code);
    }

    /// <summary>
    /// Разбирает программу.
    /// </summary>
    public Expression ParseProgram()
    {
        Expression e = ParseExpression();
        Match(TokenType.EndOfFile);

        return e;
    }

    /// <summary>
    /// Разбирает выражение.
    /// Правило:
    ///     expression = logical_or_expression ;
    /// </summary>
    private Expression ParseExpression()
    {
        return ParseAssignmentExpression();
    }

    /// <summary>
    /// Разбирает присваивание.
    /// Правила:
    ///     assignment_expression = logical_or_expression
    ///         | [ ":=", logical_or_expression ] ;
    /// </summary>
    private Expression ParseAssignmentExpression()
    {
        Expression expr = ParseLogicalOrExpression();
        while (_tokens.Peek().Type == TokenType.Assign)
        {
            _tokens.Advance();
            expr = new AssignmentExpression(expr, ParseLogicalOrExpression());
        }

        return expr;
    }

    /// <summary>
    /// Разбирает логический оператор "ИЛИ".
    /// Правила:
    ///     logical_or_expression = logical_and_expression
    ///         , { "|", logical_and_expression } ;
    /// </summary>
    private Expression ParseLogicalOrExpression()
    {
        Expression expr = ParseLogicalAndExpression();
        while (_tokens.Peek().Type == TokenType.Or)
        {
            _tokens.Advance();
            expr = new BinaryOperationExpression(expr, BinaryOperation.Or, ParseLogicalAndExpression());
        }

        return expr;
    }

    /// <summary>
    /// Разбирает логический оператор "И".
    /// Правила:
    ///     logical_and_expression = relational_expression
    ///         , { "&", relational_expression } ;
    /// </summary>
    private Expression ParseLogicalAndExpression()
    {
        Expression expr = ParseRelationalExpression();
        while (_tokens.Peek().Type == TokenType.And)
        {
            _tokens.Advance();
            expr = new BinaryOperationExpression(expr, BinaryOperation.And, ParseRelationalExpression());
        }

        return expr;
    }

    /// <summary>
    /// Разбирает операторы сравнения.
    /// Особенность: отсутствие ассоциативности.
    /// Правило:
    ///     relational_expression = additive_expression, [ ("=" | "<>" | "<" | ">" | "<=" | ">=" ), additive_expression ] ;
    /// </summary>
    /// <returns></returns>
    private Expression ParseRelationalExpression()
    {
        Expression e = ParseAdditiveExpression();
        switch (_tokens.Peek().Type)
        {
            case TokenType.Equal:
                _tokens.Advance();
                return new BinaryOperationExpression(e, BinaryOperation.Equal, ParseAdditiveExpression());
            case TokenType.NotEqual:
                _tokens.Advance();
                return new BinaryOperationExpression(e, BinaryOperation.NotEqual, ParseAdditiveExpression());
            case TokenType.LessThan:
                _tokens.Advance();
                return new BinaryOperationExpression(e, BinaryOperation.LessThan, ParseAdditiveExpression());
            case TokenType.GreaterThan:
                _tokens.Advance();
                return new BinaryOperationExpression(e, BinaryOperation.GreaterThan, ParseAdditiveExpression());
            case TokenType.LessThanOrEqual:
                _tokens.Advance();
                return new BinaryOperationExpression(e, BinaryOperation.LessThanOrEqual, ParseAdditiveExpression());
            case TokenType.GreaterThanOrEqual:
                _tokens.Advance();
                return new BinaryOperationExpression(e, BinaryOperation.GreaterThanOrEqual, ParseAdditiveExpression());
            default:
                return e;
        }
    }

    /// <summary>
    /// Разбирает операторы сложения и вычитания.
    /// Правило:
    ///     additive_expression = multiplicative_expression
    ///         , { ( "+" | "-" ), multiplicative_expression } ;
    /// </summary>
    private Expression ParseAdditiveExpression()
    {
        Expression expr = ParseMultiplicativeExpression();
        while (true)
        {
            switch (_tokens.Peek().Type)
            {
                case TokenType.Plus:
                    _tokens.Advance();
                    expr = new BinaryOperationExpression(
                        expr, BinaryOperation.Add, ParseMultiplicativeExpression()
                    );
                    break;
                case TokenType.Minus:
                    _tokens.Advance();
                    expr = new BinaryOperationExpression(
                        expr, BinaryOperation.Substract, ParseMultiplicativeExpression()
                    );
                    break;
                default:
                    return expr;
            }
        }
    }

    /// <summary>
    /// Разбирает операторы умножения и деления.
    /// Правило:
    ///     multiplicative_expression = unary_expression
    ///         , { ( "*" | "/" ), unary_expression } ;
    /// </summary>
    private Expression ParseMultiplicativeExpression()
    {
        Expression expr = ParseUnaryExpression();
        while (true)
        {
            switch (_tokens.Peek().Type)
            {
                case TokenType.Multiply:
                    _tokens.Advance();
                    expr = new BinaryOperationExpression(expr, BinaryOperation.Multiply, ParseUnaryExpression());
                    break;
                case TokenType.Divide:
                    _tokens.Advance();
                    expr = new BinaryOperationExpression(expr, BinaryOperation.Divide, ParseUnaryExpression());
                    break;
                default:
                    return expr;
            }
        }
    }

    /// <summary>
    /// Выполняет разбор унарного минуса.
    /// Правило:
    ///     unary_expression = { "-" }, primary_expression ;
    /// </summary>
    private Expression ParseUnaryExpression()
    {
        if (_tokens.Peek().Type == TokenType.Minus)
        {
            _tokens.Advance();
            return new UnaryMinusExpression(ParseUnaryExpression());
        }

        return ParsePrimaryExpression();
    }

    /// <summary>
    /// Разбирает элементарные выражения.
    /// Правило:
    ///     primary_expression = literal
    ///         | identifier
    ///         | identifier, argument_list
    ///         | expression_sequence
    ///         | if_expression
    ///         | "let", declaration_list, "in", [ expression_sequence_inner ], "end" ;
    /// </summary>
    private Expression ParsePrimaryExpression()
    {
        Token t = _tokens.Peek();
        switch (t.Type)
        {
            case TokenType.IntLiteral:
                _tokens.Advance();
                return new LiteralExpression(new Value(t.Value!.ToInt()));
            case TokenType.StringLiteral:
                _tokens.Advance();
                return new LiteralExpression(new Value(t.Value!.ToString()));
            case TokenType.OpenParenthesis:
                return ParseExpressionSequence();
            case TokenType.Identifier:
                {
                    _tokens.Advance();
                    if (_tokens.Peek().Type == TokenType.OpenParenthesis)
                    {
                        List<Expression> arguments = ParseArgumentsList();
                        return new FunctionCallExpression(t.Value!.ToString(), arguments);
                    }

                    return new VariableAccessExpression(t.Value!.ToString());
                }

            case TokenType.If:
                return ParseIfExpression();

            case TokenType.Let:
                {
                    _tokens.Advance();
                    List<Declaration> declarations = ParseDeclarationList();
                    Match(TokenType.In);
                    List<Expression> expressions = [];
                    if (_tokens.Peek().Type != TokenType.End)
                    {
                        expressions = ParseInnerExpressionSequence();
                    }

                    Match(TokenType.End);
                    return new ScopeExpression(declarations, expressions);
                }

            default:
                throw new UnexpectedLexemeException(
                    t,
                    expected:
                    [
                        TokenType.IntLiteral,
                        TokenType.StringLiteral,
                        TokenType.OpenParenthesis,
                        TokenType.Identifier,
                        TokenType.If,
                        TokenType.Let,
                    ]
                );
        }
    }

    /// <summary>
    /// Разбирает список аргументов функции.
    /// Правило:
    ///     arguments_list = "(", [ expression, { ",", expression } ], ")" ;
    /// </summary>
    private List<Expression> ParseArgumentsList()
    {
        List<Expression> arguments = [];

        Match(TokenType.OpenParenthesis); // Читаем открывающую скобку.

        // Читаем необязательный список аргументов, разделённых запятыми.
        if (_tokens.Peek().Type != TokenType.CloseParenthesis)
        {
            arguments.Add(ParseExpression());
            while (_tokens.Peek().Type == TokenType.Comma)
            {
                _tokens.Advance();
                arguments.Add(ParseExpression());
            }
        }

        Match(TokenType.CloseParenthesis); // Читаем закрывающую скобку.

        return arguments;
    }

    /// <summary>
    /// Разбор последовательности выражений в скобках.
    /// Правила:
    ///     expression_sequence = "(", [ inner_expression_sequence ], ")" ;
    /// </summary>
    private Expression ParseExpressionSequence()
    {
        Match(TokenType.OpenParenthesis);

        List<Expression> expressions = [];
        if (_tokens.Peek().Type != TokenType.CloseParenthesis)
        {
            expressions = ParseInnerExpressionSequence();
        }

        Match(TokenType.CloseParenthesis);

        return new SequenceExpression(expressions);
    }

    /// <summary>
    /// Разбор последовательности выражений.
    /// Правила:
    ///     inner_expression_sequence = expression,  { ";", expression } ;
    /// </summary>
    private List<Expression> ParseInnerExpressionSequence()
    {
        List<Expression> expressions =
        [
            ParseExpression(),
        ];

        // Читаем последующие выражения, разделённые лексемой ";".
        while (_tokens.Peek().Type == TokenType.Semicolon)
        {
            _tokens.Advance();
            expressions.Add(ParseExpression());
        }

        return expressions;
    }

    /// <summary>
    /// Разбор условного выражения.
    /// Правило:
    ///     if_expression = "if", expression, "then", expression, [ "else", expression ] ;
    /// </summary>
    private Expression ParseIfExpression()
    {
        Match(TokenType.If);
        Expression condition = ParseExpression();
        Match(TokenType.Then);
        Expression thenBranch = ParseExpression();

        Expression? elseBranch = null;
        if (_tokens.Peek().Type == TokenType.Else)
        {
            _tokens.Advance();
            elseBranch = ParseExpression();
        }

        return new IfElseExpression(condition, thenBranch, elseBranch);
    }

    /// <summary>
    /// Разбирает список объявлений символов.
    /// Правило:
    ///     declaration_list = declaration, { declaration } ;
    /// </summary>
    private List<Declaration> ParseDeclarationList()
    {
        List<Declaration> declarations =
        [
            ParseDeclaration()
        ];
        while (_tokens.Peek().Type != TokenType.In)
        {
            declarations.Add(ParseDeclaration());
        }

        return declarations;
    }

    /// <summary>
    /// Разбирает объявление символа.
    /// Правило:
    ///     declaration = variable_declaration ;
    /// </summary>
    private Declaration ParseDeclaration()
    {
        return ParseVariableDeclaration();
    }

    /// <summary>
    /// Разбирает объявление переменной.
    /// Правило:
    ///     variable_declaration = "var", identifier, [":", identifier], ":=", expression ;
    /// </summary>
    private VariableDeclaration ParseVariableDeclaration()
    {
        Match(TokenType.Var);
        string name = Match(TokenType.Identifier).Value!.ToString();

        string? typeName = null;
        if (_tokens.Peek().Type == TokenType.Colon)
        {
            _tokens.Advance();
            typeName = Match(TokenType.Identifier).Value!.ToString();
        }

        Match(TokenType.Assign);
        Expression expression = ParseExpression();

        return new VariableDeclaration(name, typeName, expression);
    }

    /// <summary>
    /// Читает ожидаемую лексему либо бросает исключение, если встретит иную лексему.
    /// </summary>
    private Token Match(TokenType expected)
    {
        Token t = _tokens.Peek();
        if (t.Type != expected)
        {
            throw new UnexpectedLexemeException(t, expected);
        }

        _tokens.Advance();
        return t;
    }
}