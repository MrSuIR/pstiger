using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Execution;
using PsTiger.Runtime;
using PsTiger.VirtualMachine;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Codegen;

/// <summary>
/// Генерирует инструкции виртуальной машины TigerVm путём обхода абстрактного синтаксического дерева (AST) программы.
/// </summary>
public class TigerVmCodegen : IAstVisitor
{
    private readonly InstructionsBuilder _builder = new();
    private CodegenSymbolsTable? _symbolsTable;

    /// <summary>
    /// Стек со ссылками на блоки после текущих циклов (while и for).
    /// Используется для генерации прерывания цикла (break).
    /// </summary>
    private readonly Stack<BasicBlock> _currentLoopFinalBlockStack = new();

    public List<Instruction> GenerateCode(Expression program)
    {
        program.Accept(this);

        if (program.ResultType != ValueType.Void)
        {
            _builder.Append(new Instruction(InstructionCode.StoreResult));
        }

        _builder.Append(new Instruction(InstructionCode.Push, 0));
        _builder.Append(new Instruction(InstructionCode.Halt));

        return _builder.Finish();
    }

    public void Visit(LiteralExpression e)
    {
        _builder.Append(new Instruction(InstructionCode.Push, e.Value));
    }

    public void Visit(BinaryOperationExpression e)
    {
        switch (e.Operation)
        {
            case BinaryOperation.Add:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.Add);
                break;
            case BinaryOperation.Subtract:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.Subtract);
                break;
            case BinaryOperation.Multiply:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.Multiply);
                break;
            case BinaryOperation.Divide:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.Divide);
                break;
            case BinaryOperation.And:
                GenerateLogicalAndCode(e);
                break;

            case BinaryOperation.Or:
                GenerateLogicalOrCode(e);
                break;

            case BinaryOperation.Equal:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.Equal);
                break;
            case BinaryOperation.NotEqual:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.NotEqual);
                break;
            case BinaryOperation.LessThan:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.Less);
                break;
            case BinaryOperation.LessThanOrEqual:
                GenerateBinaryOperationCode(e.Left, e.Right, InstructionCode.LessOrEqual);
                break;
            case BinaryOperation.GreaterThan:
                // Меняем операнды местами, потому что у нашей виртуальной машины нет инструкции Greater.
                GenerateBinaryOperationCode(e.Right, e.Left, InstructionCode.Less);
                break;
            case BinaryOperation.GreaterThanOrEqual:
                // Меняем операнды местами, потому что у нашей виртуальной машины нет инструкции GreaterOrEqual.
                GenerateBinaryOperationCode(e.Right, e.Left, InstructionCode.LessOrEqual);
                break;
            default:
                throw new NotImplementedException($"Unsupported binary operation type {e.Operation}");
        }
    }

    public void Visit(SequenceExpression e)
    {
        GenerateExpressionsSequenceCode(e.Sequence);
    }

    public void Visit(UnaryMinusExpression e)
    {
        e.Operand.Accept(this);
        _builder.Append(new Instruction(InstructionCode.Negate));
    }

    public void Visit(FunctionCallExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }

        switch (e.Function)
        {
            case BuiltinFunction builtin:
                Instruction instruction = builtin.Name switch
                {
                    Builtins.Not => new Instruction(InstructionCode.Not),
                    Builtins.Exit => new Instruction(InstructionCode.Halt),
                    _ => new Instruction(InstructionCode.CallBuiltin, builtin.Name),
                };
                _builder.Append(instruction);
                break;

            case FunctionDeclaration functionDeclaration:
                {
                    Function function = _symbolsTable!.GetFunction(functionDeclaration.Name);
                    _builder.AppendJump(InstructionCode.Call, function.Entry);
                }

                break;

            default:
                throw new NotImplementedException($"Unsupported AST subclass {e.Function.GetType()}");
        }
    }

    public void Visit(ScopeExpression e)
    {
        PushLexicalScope();

        foreach (Declaration declaration in e.Declarations)
        {
            declaration.Accept(this);
        }

        GenerateExpressionsSequenceCode(e.Expressions);

        PopLexicalScope();
    }

    public void Visit(VariableAccessExpression e)
    {
        _builder.Append(new Instruction(InstructionCode.LoadVar, e.Variable.Name));
    }

    public void Visit(AssignmentExpression e)
    {
        /*
         Пример кода, генерируемого для выражения `arr[x][y] := 10`:
           Push 10
           LoadVar "arr"
           LoadVar "x"
           LoadArray
           LoadVar "y"
           StoreArray

        Здесь StoreArray используется только один раз, чтобы сохранить значение 10 по индексу `y` в массив `arr[x]`.
         */

        e.Right.Accept(this);

        switch (e.Left)
        {
            case VariableAccessExpression variableAccess:
                _builder.Append(new Instruction(InstructionCode.StoreVar, variableAccess.Variable.Name));
                break;

            case ArrayAccessExpression arrayAccess:
                arrayAccess.Array.Accept(this);
                arrayAccess.Index.Accept(this);
                _builder.Append(new Instruction(InstructionCode.StoreArray));
                break;

            case FieldAccessExpression fieldAccess:
                fieldAccess.Record.Accept(this);
                _builder.Append(new Instruction(InstructionCode.StoreField, fieldAccess.FieldName));
                break;

            default:
                throw new NotImplementedException();
        }
    }

    public void Visit(IfElseExpression e)
    {
        if (e.ElseBranch != null)
        {
            // Конструкция if ... then ... else ... выполняется так:
            // 1) Вычисляется условие
            // 2) Если результат равен нулю, то прыгаем на ветку else
            // 3) Иначе выполняем ветку then и затем перепрыгиваем через ветку else
            BasicBlock elseBlock = _builder.CreateBasicBlock();
            BasicBlock finalBlock = _builder.CreateBasicBlock();

            e.Condition.Accept(this);
            _builder.AppendJump(InstructionCode.JumpIfFalse, elseBlock);

            e.ThenBranch.Accept(this);
            _builder.AppendJump(InstructionCode.Jump, finalBlock);

            _builder.InsertPoint = elseBlock;
            e.ElseBranch.Accept(this);
            _builder.AppendJump(InstructionCode.Jump, finalBlock);

            _builder.InsertPoint = finalBlock;
        }
        else
        {
            // Конструкция if ... then выполняется так:
            // 1) Вычисляется условие
            // 2) Если результат равен нулю, то перепрыгиваем через ветку then
            BasicBlock finalBlock = _builder.CreateBasicBlock();

            e.Condition.Accept(this);
            _builder.AppendJump(InstructionCode.JumpIfFalse, finalBlock);

            e.ThenBranch.Accept(this);
            _builder.AppendJump(InstructionCode.Jump, finalBlock);

            _builder.InsertPoint = finalBlock;
        }
    }

    public void Visit(VariableDeclaration d)
    {
        d.InitialValue.Accept(this);
        _builder.Append(new Instruction(InstructionCode.DefineVar, d.Name));
    }

    public void Visit(FunctionDeclaration d)
    {
        BasicBlock functionBlock = _builder.CreateBasicBlock();
        _symbolsTable!.DefineFunction(d.Name, functionBlock);

        BasicBlock previousBlock = _builder.InsertPoint;
        _builder.InsertPoint = functionBlock;
        try
        {
            // Создание области видимости, дочерней от области, в которой находилось объявление функции.
            PushLexicalScope();

            // Сохранение параметров со стека в переменные (в обратном порядке).
            foreach (AbstractParameterDeclaration declaration in d.Parameters.Reverse())
            {
                _builder.Append(new Instruction(InstructionCode.DefineVar, declaration.Name));
            }

            // Генерация кода для тела функции.
            d.Body.Accept(this);

            PopLexicalScope();

            // Возврат из функции.
            _builder.Append(new Instruction(InstructionCode.Return));
        }
        finally
        {
            _builder.InsertPoint = previousBlock;
        }
    }

    public void Visit(ParameterDeclaration d)
    {
    }

    public void Visit(WhileLoopExpression e)
    {
        BasicBlock loopBlock = _builder.CreateBasicBlock();
        BasicBlock finalBlock = _builder.CreateBasicBlock();
        _currentLoopFinalBlockStack.Push(finalBlock);

        // Переход в начало цикла.
        _builder.AppendJump(InstructionCode.Jump, loopBlock);
        _builder.InsertPoint = loopBlock;

        // Проверяем условие и завершаем цикл, если оно ложно.
        e.Condition.Accept(this);
        _builder.AppendJump(InstructionCode.JumpIfFalse, finalBlock);

        // Генерируем тело цикла и переход к началу цикла.
        e.LoopBody.Accept(this);
        _builder.AppendJump(InstructionCode.Jump, loopBlock);

        _currentLoopFinalBlockStack.Pop();
        _builder.InsertPoint = finalBlock;
    }

    public void Visit(ForLoopExpression e)
    {
        BasicBlock loopBlock = _builder.CreateBasicBlock();
        BasicBlock finalBlock = _builder.CreateBasicBlock();
        _currentLoopFinalBlockStack.Push(finalBlock);

        // Итератор может скрывать переменные окружающей области видимости, поэтому мы добавляем область видимости.
        PushLexicalScope();

        // Инициализация итератора цикла
        e.StartValue.Accept(this);
        _builder.Append(new Instruction(InstructionCode.DefineVar, e.Iterator.Name));

        // Переход в начало цикла
        _builder.AppendJump(InstructionCode.Jump, loopBlock);
        _builder.InsertPoint = loopBlock;

        // Проверяем значение итератора и завершаем цикл, если итератор больше своего финального значения.
        _builder.Append(new Instruction(InstructionCode.LoadVar, e.Iterator.Name));
        e.EndValue.Accept(this);
        _builder.Append(new Instruction(InstructionCode.LessOrEqual));
        _builder.AppendJump(InstructionCode.JumpIfFalse, finalBlock);

        // Генерируем тело цикла, инкремент итератора и переход к началу цикла
        e.LoopBody.Accept(this);
        _builder.Append(new Instruction(InstructionCode.LoadVar, e.Iterator.Name));
        _builder.Append(new Instruction(InstructionCode.Push, 1));
        _builder.Append(new Instruction(InstructionCode.Add));
        _builder.Append(new Instruction(InstructionCode.StoreVar, e.Iterator.Name));
        _builder.AppendJump(InstructionCode.Jump, loopBlock);

        _currentLoopFinalBlockStack.Pop();
        _builder.InsertPoint = finalBlock;
        PopLexicalScope();
    }

    public void Visit(ForIteratorDeclaration d)
    {
    }

    public void Visit(BreakLoopExpression e)
    {
        BasicBlock loopFinalBlock = _currentLoopFinalBlockStack.Peek();
        _builder.AppendJump(InstructionCode.Jump, loopFinalBlock);
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
        e.Index.Accept(this);
        _builder.Append(new Instruction(InstructionCode.LoadArray));
    }

    public void Visit(ArrayLiteralExpression e)
    {
        e.Size.Accept(this);
        e.InitialValue.Accept(this);
        _builder.Append(new Instruction(InstructionCode.CreateArray));
    }

    public void Visit(RecordTypeExpression e)
    {
    }

    public void Visit(FieldDeclaration d)
    {
    }

    public void Visit(RecordLiteralExpression e)
    {
        _builder.Append(new Instruction(InstructionCode.Push, Value.NewRecord()));
        foreach (FieldInitializer initializer in e.Initializers)
        {
            initializer.Accept(this);
        }
    }

    public void Visit(FieldInitializer e)
    {
        e.Value.Accept(this);
        _builder.Append(new Instruction(InstructionCode.InitField, e.Name));
    }

    public void Visit(FieldAccessExpression e)
    {
        e.Record.Accept(this);
        _builder.Append(new Instruction(InstructionCode.LoadField, e.FieldName));
    }

    private void GenerateExpressionsSequenceCode(IReadOnlyList<Expression> sequence)
    {
        for (int i = 0, iMax = sequence.Count - 1; i <= iMax; ++i)
        {
            Expression expression = sequence[i];
            expression.Accept(this);

            // Отбрасываем результат всех выражений, кроме последнего.
            if (i != iMax && expression.ResultType != ValueType.Void)
            {
                _builder.Append(new Instruction(InstructionCode.Pop));
            }
        }
    }

    private void GenerateBinaryOperationCode(Expression left, Expression right, InstructionCode code)
    {
        left.Accept(this);
        right.Accept(this);
        _builder.Append(new Instruction(code));
    }

    private void GenerateLogicalAndCode(BinaryOperationExpression e)
    {
        // Логическое "И" вычисляется по короткой схеме: если первый операнд обращается в "ЛОЖЬ",
        //  то второй операнд не вычисляется.
        BasicBlock shortCircuitBlock = _builder.CreateBasicBlock();
        BasicBlock finalBlock = _builder.CreateBasicBlock();

        // Вычисляем первый операнд.
        e.Left.Accept(this);

        // Переходим к короткой схеме, если первый операнд обращается в "ЛОЖЬ".
        _builder.AppendJump(InstructionCode.JumpIfFalse, shortCircuitBlock);

        // Иначе вычисляем второй операнд.
        // Затем используем операцию "X <> 0", чтобы привести "X" к булеву значению (1 или 0).
        e.Right.Accept(this);
        _builder.Append(new Instruction(InstructionCode.Push, 0));
        _builder.Append(new Instruction(InstructionCode.NotEqual));
        _builder.AppendJump(InstructionCode.Jump, finalBlock);

        // Выполняем короткую схему вычислений: левый операнд обратился в "ЛОЖЬ", и результат будет "ЛОЖЬ".
        _builder.InsertPoint = shortCircuitBlock;
        _builder.Append(new Instruction(InstructionCode.Push, 0));
        _builder.AppendJump(InstructionCode.Jump, finalBlock);

        _builder.InsertPoint = finalBlock;
    }

    private void GenerateLogicalOrCode(BinaryOperationExpression e)
    {
        // Логическое "ИЛИ" вычисляется по короткой схеме: если первый операнд обращается в "ИСТИНУ",
        //  то второй операнд не вычисляется.
        BasicBlock shortCircuitBlock = _builder.CreateBasicBlock();
        BasicBlock finalBlock = _builder.CreateBasicBlock();

        // Вычисляем первый операнд.
        e.Left.Accept(this);

        // Переходим к короткой схеме, если первый операнд в "ИСТИНУ".
        _builder.AppendJump(InstructionCode.JumpIfTrue, shortCircuitBlock);

        // Иначе вычисляем второй операнд.
        // Затем используем операцию "X <> 0", чтобы привести "X" к булеву значению (1 или 0).
        e.Right.Accept(this);
        _builder.Append(new Instruction(InstructionCode.Push, 0));
        _builder.Append(new Instruction(InstructionCode.NotEqual));
        _builder.AppendJump(InstructionCode.Jump, finalBlock);

        // Выполняем короткую схему вычислений: левый операнд обратился в "ИСТИНУ", и результат будет "ИСТИНА".
        _builder.InsertPoint = shortCircuitBlock;
        _builder.Append(new Instruction(InstructionCode.Push, 1));
        _builder.AppendJump(InstructionCode.Jump, finalBlock);

        _builder.InsertPoint = finalBlock;
    }

    /// <summary>
    /// Добавляет лексическую область видимости в стек.
    /// </summary>
    private void PushLexicalScope()
    {
        int parentScopeDepth = _symbolsTable?.Depth ?? 0;
        _symbolsTable = new CodegenSymbolsTable(_symbolsTable);
        _builder.Append(new Instruction(InstructionCode.PushVars, parentScopeDepth));
    }

    /// <summary>
    /// Убирает лексическую область видимости из стека.
    /// </summary>
    private void PopLexicalScope()
    {
        _builder.Append(new Instruction(InstructionCode.PopVars));
        _symbolsTable = _symbolsTable!.Parent;
    }
}