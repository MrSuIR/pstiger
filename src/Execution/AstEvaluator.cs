using PsTiger.Ast;
using PsTiger.Ast.Expressions;
using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

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
            case BinaryOperation.Equal:
                _values.Push(EvaluationUtil.CompareValues(left, right, (i1, i2) => i1 == i2, (s1, s2) => s1 == s2));
                break;
            case BinaryOperation.NotEqual:
                _values.Push(EvaluationUtil.CompareValues(left, right, (i1, i2) => i1 != i2, (s1, s2) => s1 != s2));
                break;
            case BinaryOperation.LessThan:
                _values.Push(EvaluationUtil.CompareValues(
                    left,
                    right,
                    (i1, i2) => i1 < i2,
                    (s1, s2) => string.CompareOrdinal(s1, s2) < 0)
                );
                break;
            case BinaryOperation.GreaterThan:
                _values.Push(EvaluationUtil.CompareValues(
                    left,
                    right,
                    (i1, i2) => i1 > i2,
                    (s1, s2) => string.CompareOrdinal(s1, s2) > 0)
                );
                break;
            case BinaryOperation.LessThanOrEqual:
                _values.Push(EvaluationUtil.CompareValues(
                    left,
                    right,
                    (i1, i2) => i1 <= i2,
                    (s1, s2) => string.CompareOrdinal(s1, s2) <= 0)
                );
                break;
            case BinaryOperation.GreaterThanOrEqual:
                _values.Push(EvaluationUtil.CompareValues(
                    left,
                    right,
                    (i1, i2) => i1 >= i2,
                    (s1, s2) => string.CompareOrdinal(s1, s2) >= 0)
                );
                break;
            default:
                throw new NotImplementedException($"Unknown binary operation {e.Operation}");
        }
    }


    public void Visit(SequenceExpression e)
    {
        // Вычисляем все выражения последовательно, но сохраняем только последний результат.
        _values.Push(new Value());
        foreach (Expression nested in e.Sequence)
        {
            _values.Pop();
            nested.Accept(this);
        }
    }

    public void Visit(UnaryMinusExpression e)
    {
        e.Operand.Accept(this);

        int value = _values.Pop().AsInt();
        _values.Push(new Value(-value));
    }
}