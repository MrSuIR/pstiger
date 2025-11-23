using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Execution;

/// <summary>
/// Интерпретирует ранее разобранную программу, используя её AST.
/// </summary>
/// <exception cref="InvalidOperationException">Бросается при ошибке в процессе вычислений.</exception>
public class AstEvaluator : IAstVisitor
{
    /// <summary>
    /// Словарь встроенных функций языка.
    /// </summary>
    private readonly IReadOnlyDictionary<string, BuiltinFunction> _builtins;

    /// <summary>
    /// В стек временно складываются результаты вычисления операндов текущей операции.
    /// </summary>
    private readonly Stack<Value> _values = [];

    /// <summary>
    /// Таблица переменных, доступных в текущей области видимости с учётом родительских областей видимости.
    /// </summary>
    private VariablesTable _variables = new();

    public AstEvaluator(IReadOnlyDictionary<string, BuiltinFunction> builtins)
    {
        _builtins = builtins;
    }

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
        _values.Push(Value.Void);
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

    public void Visit(FunctionCallExpression e)
    {
        BuiltinFunction function = _builtins[e.Name];

        // Вычисляем аргументы функции.
        List<Value> arguments = [];
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
            arguments.Add(_values.Pop());
        }

        // Вызываем функцию и сохраняем результат в стеке.
        Value result = function.Invoke(arguments);
        _values.Push(result);
    }

    public void Visit(ScopeExpression e)
    {
        _variables = new VariablesTable(_variables);
        try
        {
            foreach (Declaration declaration in e.Declarations)
            {
                declaration.Accept(this);
            }

            _values.Push(Value.Void);
            foreach (Expression nested in e.Expressions)
            {
                _values.Pop();
                nested.Accept(this);
            }
        }
        finally
        {
            _variables = _variables.Parent ?? throw new InvalidOperationException("Cannot rollback to parent scope");
        }
    }

    public void Visit(VariableAccessExpression e)
    {
        _values.Push(_variables.GetVariable(e.Name));
    }

    public void Visit(VariableDeclaration e)
    {
        e.InitialValue.Accept(this);

        Value initialValue = _values.Pop();
        _variables.DefineVariable(e.Name, initialValue);
    }

    public void Visit(AssignmentExpression e)
    {
        e.Right.Accept(this);
        Value value = _values.Pop();

        if (e.Left is VariableAccessExpression variable)
        {
            _variables.AssignVariable(variable.Name, value);
        }
        else
        {
            throw new InvalidOperationException("Assignment expression must be a variable access");
        }

        _values.Push(Value.Void);
    }
}