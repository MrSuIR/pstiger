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
        // NOTE: Логические операторы реализуют вычисления по короткой схеме,
        //  поэтому мы используем локальные функции для «ленивого» вычисления операндов.
        _values.Push(EvaluationUtil.ApplyBinaryOperation(e.Operation, EvaluateLeft, EvaluateRight));
        return;

        // Локальная функция, вычисляющая левый операнд.
        Value EvaluateLeft()
        {
            e.Left.Accept(this);
            return _values.Pop();
        }

        // Локальная функция, вычисляющая правый операнд.
        Value EvaluateRight()
        {
            e.Right.Accept(this);
            return _values.Pop();
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