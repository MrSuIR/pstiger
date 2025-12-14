using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Execution.Assignment;
using PsTiger.Execution.Data;
using PsTiger.Execution.Exceptions;
using PsTiger.Runtime;

namespace PsTiger.Execution;

/// <summary>
/// Интерпретирует ранее разобранную программу, используя её AST.
/// </summary>
/// <exception cref="InvalidOperationException">Бросается при ошибке в процессе вычислений.</exception>
public class AstEvaluator : IAstVisitor
{
    /// <summary>
    /// В стек временно складываются результаты вычисления операндов текущей операции.
    /// </summary>
    private readonly Stack<Value> _values = [];

    /// <summary>
    /// Таблица переменных, доступных в текущей области видимости с учётом родительских областей видимости.
    /// </summary>
    private VariablesTable _variables = new();

    /// <summary>
    /// Таблица захваченных контекстов функций.
    /// </summary>
    private FunctionCapturedContextTable _functionCapturedContext = new();

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
        // Выполняем вызов функции в зависимости от её типа.
        switch (e.Function)
        {
            case BuiltinFunction builtinFunction:
                InvokeBuiltinFunction(e, builtinFunction);
                break;
            case FunctionDeclaration function:
                InvokeFunction(e, function);
                break;
            default:
                throw new InvalidOperationException($"Unknown function subclass {e.Function.GetType()}");
        }
    }

    public void Visit(ScopeExpression e)
    {
        // Добавляем область видимости переменных и сохраняем контекст для захвата переменных вложенными функциями.
        _variables = new VariablesTable(_variables);
        _functionCapturedContext = new FunctionCapturedContextTable(_functionCapturedContext);
        try
        {
            // Последовательно применяем все объявления этой области видимости.
            foreach (Declaration declaration in e.Declarations)
            {
                declaration.Accept(this);
            }

            // Последовательно вычисляем все выражения в области видимости.
            _values.Push(Value.Void);
            foreach (Expression nested in e.Expressions)
            {
                _values.Pop();
                nested.Accept(this);
            }
        }
        finally
        {
            // Восстанавливаем прежнюю область видимости и контекст для захвата переменных.
            _variables = _variables.Parent ?? throw new InvalidOperationException(
                "Cannot rollback to parent variables table"
            );
            _functionCapturedContext = _functionCapturedContext.Parent ?? throw new InvalidOperationException(
                "Cannot rollback to parent captured context table"
            );
        }
    }

    public void Visit(VariableAccessExpression e)
    {
        _values.Push(_variables.GetVariable(e.Name));
    }

    public void Visit(AssignmentExpression e)
    {
        // Вычисляем правую часть выражения до присваивания.
        e.Right.Accept(this);
        Value value = _values.Pop();

        IValueAccessor accessor = CreateValueAccessor(e.Left);
        accessor.Store(value);

        // Присваивание не возвращает значения.
        _values.Push(Value.Void);
    }

    public void Visit(IfElseExpression e)
    {
        // Вычисляем условие.
        e.Condition.Accept(this);
        int condition = _values.Pop().AsInt();

        // Выполняем ветку then, если условие истинно (не равно 0).
        if (condition != 0)
        {
            e.ThenBranch.Accept(this);
        }
        else
        {
            // Выполняем необязательную ветку else, если условие ложно (равно 0).
            if (e.ElseBranch != null)
            {
                e.ElseBranch.Accept(this);
            }
            else
            {
                _values.Push(Value.Void);
            }
        }
    }

    public void Visit(VariableDeclaration d)
    {
        d.InitialValue.Accept(this);

        Value initialValue = _values.Pop();
        _variables.DefineVariable(d.Name, initialValue);
    }

    public void Visit(FunctionDeclaration d)
    {
        _functionCapturedContext.CaptureVariablesTable(d, _variables);
    }

    public void Visit(ParameterDeclaration d)
    {
    }

    public void Visit(WhileLoopExpression e)
    {
        // Цикл ничего не возвращает - сразу добавляем в стек значение Void.
        _values.Push(Value.Void);
        try
        {
            while (true)
            {
                // Выполняем цикл, пока условие истинно (не равно 0).
                e.Condition.Accept(this);
                int condition = _values.Pop().AsInt();
                if (condition == 0)
                {
                    break;
                }

                // Выполняем тело цикла, перед этим выбрасываем из стека значение Void, сохранённое на предыдущей итерации.
                _values.Pop();
                e.LoopBody.Accept(this);
            }
        }
        catch (BreakLoopException)
        {
        }
    }

    public void Visit(ForLoopExpression e)
    {
        // Вычисляем начальное и конечное значение заранее - до объявления переменной-итератора.
        e.StartValue.Accept(this);
        int firstValue = _values.Pop().AsInt();

        e.EndValue.Accept(this);
        int lastValue = _values.Pop().AsInt();

        // Добавляем область видимости переменных.
        _variables = new VariablesTable(_variables);
        try
        {
            // Объявляем переменную-итератор.
            _variables.DefineVariable(e.Iterator.Name, new Value(firstValue));

            // Цикл ничего не возвращает - сразу добавляем в стек значение Void.
            _values.Push(Value.Void);
            for (int i = firstValue; i <= lastValue; ++i)
            {
                // Выполняем цикл с новым значением итератора, а перед этим выбрасываем из стека значение Void,
                // сохранённое на предыдущей итерации.
                _values.Pop();
                _variables.AssignVariable(e.Iterator.Name, new Value(i));
                e.LoopBody.Accept(this);
            }
        }
        catch (BreakLoopException)
        {
        }
        finally
        {
            // Восстанавливаем прежнюю область видимости.
            _variables = _variables.Parent ?? throw new InvalidOperationException(
                "Cannot rollback to parent variables table"
            );
        }
    }

    public void Visit(ForIteratorDeclaration d)
    {
    }

    public void Visit(BreakLoopExpression e)
    {
        _values.Push(Value.Void);
        throw new BreakLoopException();
    }

    public void Visit(TypeDeclaration d)
    {
    }

    public void Visit(NamedTypeExpression e)
    {
    }

    public void Visit(ArrayTypeExpression e)
    {
    }

    public void Visit(ArrayAccessExpression e)
    {
        e.Array.Accept(this);
        Value array = _values.Pop();

        e.Index.Accept(this);
        int index = _values.Pop().AsInt();

        _values.Push(array.GetElement(index));
    }

    public void Visit(ArrayLiteralExpression e)
    {
        e.Size.Accept(this);
        int size = _values.Pop().AsInt();

        e.InitialValue.Accept(this);
        Value initialValue = _values.Pop();

        _values.Push(Value.NewArray(size, initialValue));
    }

    public void Visit(RecordTypeExpression e)
    {
    }

    public void Visit(FieldDeclaration d)
    {
    }

    public void Visit(RecordLiteralExpression e)
    {
        Value record = Value.NewRecord();
        foreach (FieldInitializer initializer in e.Initializers)
        {
            initializer.Value.Accept(this);
            Value value = _values.Pop();
            record.SetField(initializer.Name, value);
        }

        _values.Push(record);
    }

    public void Visit(FieldInitializer e)
    {
    }

    public void Visit(FieldAccessExpression e)
    {
        e.Record.Accept(this);
        Value record = _values.Pop();

        _values.Push(record.GetField(e.FieldName));
    }

    private void InvokeBuiltinFunction(FunctionCallExpression e, BuiltinFunction function)
    {
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

    private void InvokeFunction(FunctionCallExpression e, FunctionDeclaration function)
    {
        VariablesTable capturedVariables = _functionCapturedContext.GetCapturedVariablesTable(function);
        VariablesTable variables = new(capturedVariables);
        VariablesTable oldVariables = _variables;

        // Вычисляем аргументы функции и записываем их в таблицу переменных.
        for (int i = 0, iMax = function.Parameters.Count; i < iMax; ++i)
        {
            e.Arguments[i].Accept(this);
            Value argument = _values.Pop();

            string name = function.Parameters[i].Name;
            variables.DefineVariable(name, argument);
        }

        _variables = variables;
        try
        {
            // Вызываем функцию и сохраняем результат в стеке.
            function.Body.Accept(this);
        }
        finally
        {
            _variables = oldVariables;
        }
    }

    private IValueAccessor CreateValueAccessor(Expression lvalue)
    {
        switch (lvalue)
        {
            case VariableAccessExpression variable:
                return new VariableAccessor(_variables, variable.Name);

            case ArrayAccessExpression arrayAccess:
                {
                    IValueAccessor array = CreateValueAccessor(arrayAccess.Array);

                    arrayAccess.Index.Accept(this);
                    int index = _values.Pop().AsInt();

                    return new ArrayElementAccessor(array, index);
                }

            case FieldAccessExpression fieldAccess:
                {
                    IValueAccessor record = CreateValueAccessor(fieldAccess.Record);
                    return new RecordFieldAccessor(record, fieldAccess.FieldName);
                }

            default:
                throw new InvalidOperationException("Assignment expression is not a lvalue");
        }
    }
}