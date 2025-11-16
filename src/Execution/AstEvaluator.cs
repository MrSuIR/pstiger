using PsTiger.Ast;
using PsTiger.Ast.Expressions;
using PsTiger.Runtime;

namespace PsTiger.Execution;

public class AstEvaluator : IAstVisitor
{
    private readonly Stack<Value> _values = [];

    public Value Evaluate(AstNode node)
    {
        if (_values.Count > 0)
        {
            throw new InvalidOperationException(
                $"Evaluation stack must be empty, but contains {_values.Count} values: {string.Join(", ", _values)}"
            );
        }

        node.Accept(this);

        return _values.Count switch
        {
            0 => throw new InvalidOperationException(
                "Evaluator logical error: the stack has no evaluation result"
            ),
            > 1 => throw new InvalidOperationException(
                $"Evaluator logical error: expected 1 value, got {_values.Count} values: {string.Join(", ", _values)}"
            ),
            _ => _values.Pop(),
        };
    }

    public void Visit(LiteralExpression e)
    {
        _values.Push(e.Value);
    }

    public void Visit(BinaryOperationExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
        Value right = _values.Pop();
        Value left = _values.Pop();

        switch (e.Operation)
        {
            case BinaryOperation.Plus:
                _values.Push(new Value(left.AsInt() + right.AsInt()));
                break;
            case BinaryOperation.Minus:
                _values.Push(new Value(left.AsInt() - right.AsInt()));
                break;
            case BinaryOperation.Multiply:
                _values.Push(new Value(left.AsInt() * right.AsInt()));
                break;
            case BinaryOperation.Divide:
                _values.Push(new Value(left.AsInt() / right.AsInt()));
                break;
            default:
                throw new NotImplementedException($"Unknown binary operation {e.Operation}");
        }
    }
}