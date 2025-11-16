using System.Linq.Expressions;

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
    /// Разбирает выражение.
    /// Правило:
    ///     expression = logical_or_expression
    ///         | "(", exression_sequence, ")" ;
    /// </summary>
    // TODO: Реализовать разбор последовательности выражений.
    public Expression ParseExpression()
    {
        if (_tokens.Peek().Type == TokenType.OpenParenthesis)
        {
            return ParseExpressionSequence();
        }

        return ParseLogicalOrExpression();
    }

    private Expression ParseExpressionSequence()
    {
        List<Expression> expressions = [];

        Match(TokenType.OpenParenthesis);
        if (_tokens.Peek().Type != TokenType.CloseParenthesis)
        {
            expressions.Add(ParseExpression());

            // Читаем последующие выражения, разделённые лексемой ";".
            while (_tokens.Peek().Type == TokenType.Semicolon)
            {
                _tokens.Advance();
                expressions.Add(ParseExpression());
            }
        }

        Match(TokenType.CloseParenthesis);

        return new SequenceExpression(expressions);
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

    // TODO: Реализовать разбор.
    private Expression ParseRelationalExpression()
    {
        return ParseAdditiveExpression();
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
                    expr = new BinaryOperationExpression(expr, BinaryOperation.Plus, ParseMultiplicativeExpression());
                    break;
                case TokenType.Minus:
                    _tokens.Advance();
                    expr = new BinaryOperationExpression(expr, BinaryOperation.Minus, ParseMultiplicativeExpression());
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

    // TODO: Реализовать разбор.
    private Expression ParseUnaryExpression()
    {
        return ParsePrimaryExpression();
    }

    /// <summary>
    /// Разбирает элементарные выражения.
    /// Правило:
    ///     primary_expression = literal
    ///         | identifier, argument_list ;
    /// </summary>
    // TODO: Реализовать разбор вызова функций.
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
            default:
                throw new UnexpectedLexemeException(t, [TokenType.IntLiteral, TokenType.StringLiteral]);
        }
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