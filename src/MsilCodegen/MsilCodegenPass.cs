using System.Reflection;
using System.Reflection.Emit;

using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.MsilCodegen;

public class MsilCodegenPass : IAstVisitor
{
    private readonly ModuleBuilder _moduleBuilder;
    private readonly TigerTypeMapper _typeMapper;
    private readonly BuiltinFunctionEmitter _builtinFunctionEmitter;

    /// <summary>
    /// Тип Program будущей программы.
    /// </summary>
    private TypeBuilder _programTypeBuilder = null!;

    /// <summary>
    /// Генератор инструкций для текущего метода.
    /// </summary>
    private ILGenerator _il = null!;

    // Текущая область видимости переменных.
    private LocalVariablesScope _localVariables;

    // Стек меток конца цикла для прерывания цикла (break).
    private readonly Stack<Label> _loopEndsStack;

    // Словарь методов, соответствующих пользовательским функциям исходной программы.
    private readonly Dictionary<string, MethodBuilder> _userFunctionMethodsMap;

    public MsilCodegenPass(ModuleBuilder moduleBuilder)
    {
        _moduleBuilder = moduleBuilder;
        _typeMapper = new TigerTypeMapper();
        _builtinFunctionEmitter = new BuiltinFunctionEmitter();
        _localVariables = new LocalVariablesScope();
        _loopEndsStack = new Stack<Label>();
        _userFunctionMethodsMap = new Dictionary<string, MethodBuilder>();
    }

    /// <summary>
    /// Создаёт класс Program и метод Main(), возвращает MethodBuilder для метода Main().
    /// </summary>
    public MethodBuilder GenerateProgramCode(Expression program)
    {
        // Создаём класс Program.
        _programTypeBuilder = _moduleBuilder.DefineType(
            "Program",
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class
        );

        MethodBuilder mainMethod = DefineProgramClassMethod("Main", typeof(void), Type.EmptyTypes);
        _il = mainMethod.GetILGenerator();

        // Начинаем глобальную область видимости переменных.
        BeginScope();

        program.Accept(this);

        // В Tiger программа может быть выражением, возвращающим значение.
        // В .NET функция Main должна иметь тип void.
        if (program.ResultType != ValueType.Void)
        {
            _il.Emit(OpCodes.Pop);
        }

        // Завершаем метод Main инструкцией ret.
        _il.Emit(OpCodes.Ret);

        // Завершаем глобальную область видимости переменных.
        EndScope();

        // Завершаем создание класса Program.
        _programTypeBuilder.CreateType();

        return mainMethod;
    }

    public void Visit(LiteralExpression e)
    {
        if (e.Type == ValueType.Int)
        {
            _il.Emit(OpCodes.Ldc_I4, e.Value.AsInt());
        }
        else if (e.Type == ValueType.String)
        {
            _il.Emit(OpCodes.Ldstr, e.Value.AsString());
        }
        else
        {
            throw new NotImplementedException($"Literal of type {e.Type} are not supported yet.");
        }
    }

    public void Visit(BinaryOperationExpression e)
    {
        if (e.Left.ResultType == ValueType.String && e.Right.ResultType == ValueType.String)
        {
            EmitStringsBinaryOperation(e);
        }
        else
        {
            EmitIntegersBinaryOperation(e);
        }
    }

    public void Visit(SequenceExpression e)
    {
        // Генерируем код для каждого выражения в последовательности.
        // Результаты всех выражений, кроме последнего, отбрасываются.
        for (int i = 0, iEnd = e.Sequence.Count; i < iEnd; i++)
        {
            Expression expr = e.Sequence[i];
            expr.Accept(this);

            // Если это не последнее выражение и его тип не void, отбрасываем результат.
            if (i != iEnd - 1 && expr.ResultType != ValueType.Void)
            {
                _il.Emit(OpCodes.Pop);
            }
        }
    }

    public void Visit(UnaryMinusExpression e)
    {
        // Генерируем код для вычисления операнда.
        e.Operand.Accept(this);

        // Применяем унарный минус.
        _il.Emit(OpCodes.Neg);
    }

    public void Visit(FunctionCallExpression e)
    {
        // Генерируем код для аргументов (они должны быть добавлены на стек в порядке перечисления).
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }

        // Генерируем вызов встроенной либо пользовательской функции.
        if (e.Function is BuiltinFunction)
        {
            _builtinFunctionEmitter.EmitCallBuiltinFunction(e.Name, _il);
        }
        else
        {
            if (!_userFunctionMethodsMap.TryGetValue(e.Name, out MethodBuilder? method))
            {
                throw new InvalidOperationException($"Cannot find .NET method for function with name {e.Name}");
            }

            _il.Emit(OpCodes.Call, method);
        }
    }

    public void Visit(ScopeExpression e)
    {
        // Начинаем новую область видимости переменных.
        BeginScope();

        // Обрабатываем объявления, чтобы добавить переменные и функции.
        foreach (Declaration declaration in e.Declarations)
        {
            declaration.Accept(this);
        }

        // Обрабатываем выражения в области видимости.
        // Результаты всех выражений, кроме последнего, отбрасываются.
        for (int i = 0, iEnd = e.Expressions.Count; i < iEnd; i++)
        {
            Expression expr = e.Expressions[i];
            expr.Accept(this);

            // Если это не последнее выражение и его тип не void, отбрасываем результат.
            if (i != iEnd - 1 && expr.ResultType != ValueType.Void)
            {
                _il.Emit(OpCodes.Pop);
            }
        }

        // Завершаем область видимости переменных.
        EndScope();
    }

    public void Visit(VariableAccessExpression e)
    {
        // Добавляем чтение переменной.
        LocalBuilder local = FindVariable(e.Name);
        _il.Emit(OpCodes.Ldloc, local);
    }

    public void Visit(AssignmentExpression e)
    {
        switch (e.Left)
        {
            case VariableAccessExpression variableAccess:
                {
                    // Сохраняем вычисленное выражение в переменной.
                    LocalBuilder local = FindVariable(variableAccess.Variable.Name);
                    e.Right.Accept(this);
                    _il.Emit(OpCodes.Stloc, local);
                    break;
                }

            case ArrayAccessExpression arrayAccess:
                {
                    Type elementType = _typeMapper.MapType(arrayAccess.ResultType);
                    arrayAccess.Array.Accept(this);
                    arrayAccess.Index.Accept(this);
                    e.Right.Accept(this);
                    _il.Emit(OpCodes.Stelem, elementType);
                    break;
                }

            default:
                throw new NotImplementedException($"Assignment to {e.Left.GetType()} lvalue is not implemented yet");
        }
    }

    public void Visit(IfElseExpression e)
    {
        Label endLabel = _il.DefineLabel();
        if (e.ElseBranch != null)
        {
            // Вычисляем условие — если оно равно 0, то пропускаем ветку then и переходим к ветке else.
            Label elseLabel = _il.DefineLabel();
            e.Condition.Accept(this);
            _il.Emit(OpCodes.Brfalse, elseLabel);

            // Генерируем код для ветки then.
            e.ThenBranch.Accept(this);
            _il.Emit(OpCodes.Br, endLabel);

            // Блок else: генерируем код ветки else.
            _il.MarkLabel(elseLabel);
            e.ElseBranch.Accept(this);
        }
        else
        {
            // Вычисляем условие — если оно равно 0, то пропускаем ветку then.
            e.Condition.Accept(this);
            _il.Emit(OpCodes.Brfalse, endLabel);
            e.ThenBranch.Accept(this);
        }

        _il.MarkLabel(endLabel);
    }

    public void Visit(VariableDeclaration d)
    {
        EmitDefineVariable(d.Name, d.ResultType, d.InitialValue);
    }

    public void Visit(FunctionDeclaration d)
    {
        // Создаём метод и сохраняем его для использования при последующих вызовах (включая рекурсивные вызовы).
        MethodBuilder method = DefineProgramClassMethod(
            GetUserFunctionMethodName(d.Name),
            _typeMapper.MapType(d.ResultType),
            d.Parameters.Select(p => _typeMapper.MapType(p.ResultType)).ToArray()
        );
        _userFunctionMethodsMap[d.Name] = method;

        // Сохраняем прежний генератор MSIL.
        ILGenerator previousIl = _il;

        try
        {
            // Меняем генератор MSIL и добавляем область видимости.
            _il = method.GetILGenerator();
            BeginScope();

            for (int i = 0, iEnd = d.Parameters.Count; i < iEnd; ++i)
            {
                AbstractParameterDeclaration param = d.Parameters[i];
                EmitDefineParameter(param.Name, param.ResultType, i);
            }

            // Генерируем код для тела функции.
            d.Body.Accept(this);

            // Добавляем возврат из функции.
            _il.Emit(OpCodes.Ret);
        }
        finally
        {
            // Убираем область видимости и восстанавливаем прежний генератор MSIL.
            EndScope();
            _il = previousIl;
        }
    }

    public void Visit(ParameterDeclaration d)
    {
        // Ничего не делаем — параметр уже обработан при обходе объявления функции.
    }

    public void Visit(WhileLoopExpression e)
    {
        Label loopStart = _il.DefineLabel(); // Метка проверки условия.
        Label loopEnd = _il.DefineLabel(); // Метка конца цикла.
        _loopEndsStack.Push(loopEnd);

        // Начало цикла: вычисляем условие и завершаем цикл, если оно ложно.
        _il.MarkLabel(loopStart);
        e.Condition.Accept(this);
        _il.Emit(OpCodes.Brfalse, loopEnd);

        // Генерируем тело цикла и переходим к началу цикла.
        e.LoopBody.Accept(this);
        _il.Emit(OpCodes.Br, loopStart);

        // Метка конца цикла (выход).
        _il.MarkLabel(loopEnd);
        _loopEndsStack.Pop();
    }

    public void Visit(ForLoopExpression e)
    {
        Label loopStart = _il.DefineLabel(); // Метка проверки условия.
        Label loopEnd = _il.DefineLabel(); // Метка конца цикла.
        _loopEndsStack.Push(loopEnd);

        // Начинаем новую область видимости, объявляем итератор и вычисляем его начальное значение.
        BeginScope();
        LocalBuilder iterator = EmitDefineVariable(e.Iterator.Name, e.Iterator.ResultType, e.StartValue);

        // Блок начала цикла (проверки условия).
        // Сравниваем итератор с конечным значением: если итератор больше, то переходим к концу цикла.
        _il.MarkLabel(loopStart);
        _il.Emit(OpCodes.Ldloc, iterator);
        e.EndValue.Accept(this);
        _il.Emit(OpCodes.Cgt);
        _il.Emit(OpCodes.Brtrue, loopEnd);

        // Генерируем тело цикла
        e.LoopBody.Accept(this);

        // Инкремент итератора: загружаем значение, добавляем 1, сохраняем.
        _il.Emit(OpCodes.Ldloc, iterator);
        _il.Emit(OpCodes.Ldc_I4_1);
        _il.Emit(OpCodes.Add);
        _il.Emit(OpCodes.Stloc, iterator);

        // Снова переходим к началу цикла.
        _il.Emit(OpCodes.Br, loopStart);

        // Блок после завершения цикла.
        _il.MarkLabel(loopEnd);
        _loopEndsStack.Pop();

        // Завершаем область видимости.
        EndScope();
    }

    public void Visit(ForIteratorDeclaration d)
    {
    }

    public void Visit(BreakLoopExpression e)
    {
        Label loopEnd = _loopEndsStack.Peek();
        _il.Emit(OpCodes.Br, loopEnd);
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
        Type elementType = _typeMapper.MapType(e.ResultType);

        e.Array.Accept(this);
        e.Index.Accept(this);
        _il.Emit(OpCodes.Ldelem, elementType);
    }

    /// <summary>
    /// Создаёт новый массив, оставляет на вершине стека его значение.
    /// </summary>
    /// <remarks>
    /// Мы могли бы использовать Array.Fill() для заполнения массива начальным значением,
    ///  но тогда многомерные массивы будут инициализированы одним и тем же подмассивом.
    /// </remarks>
    public void Visit(ArrayLiteralExpression e)
    {
        // Определяем тип массива и элемента массива.
        Type arrayType = _typeMapper.MapType(e.ResultType);
        Type elementType = arrayType.GetElementType()!;

        // Создаём массив заданного размера.
        // На вершине стека остаётся ссылка на массив.
        e.Size.Accept(this);
        _il.Emit(OpCodes.Newarr, elementType);

        // Сохраняем ссылку на массив в локальную переменную.
        // На вершине стека всё ещё ссылка на массив.
        LocalBuilder arrayLocal = _il.DeclareLocal(arrayType);
        _il.Emit(OpCodes.Dup);
        _il.Emit(OpCodes.Stloc, arrayLocal);

        // Сохраняем размер массива в локальную переменную.
        // На вершине стека всё ещё ссылка на массив.
        _il.Emit(OpCodes.Dup);
        LocalBuilder sizeLocal = SaveArraySizeToLocal();

        // Вычисляем однократно начальное значение и сохраняем в локальную переменную.
        // На вершине стека всё ещё ссылка на массив.
        LocalBuilder initialValueLocal = _il.DeclareLocal(elementType);
        e.InitialValue.Accept(this);
        _il.Emit(OpCodes.Stloc, initialValueLocal);

        // Далее заполняем массив поэлементно, на каждой итерации выполняя
        //  глубокое копирование начального значения.
        EmitForEachIndex(sizeLocal, localIndex =>
        {
            _il.Emit(OpCodes.Ldloc, arrayLocal);
            _il.Emit(OpCodes.Ldloc, localIndex);
            _il.Emit(OpCodes.Ldloc, initialValueLocal);
            EmitDeepCopy(elementType);
            _il.Emit(OpCodes.Stelem, elementType);
        });
    }

    public void Visit(RecordTypeExpression e)
    {
    }

    public void Visit(FieldDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(RecordLiteralExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(FieldInitializer e)
    {
        throw new NotImplementedException();
    }

    public void Visit(FieldAccessExpression e)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Заменяет значение на вершине стека на глубокую копию этого значения.
    /// </summary>
    /// <remarks>
    /// Значения типов int и string не копируются.
    /// Значения массов копируются поэлементно, глубокое копирование применяется к ним рекурсивно.
    /// </remarks>
    private void EmitDeepCopy(Type valueType)
    {
        // Целые числа и строки неизменяемы, им не требуется копирование.
        if (valueType.IsValueType || valueType == typeof(string))
        {
            return;
        }

        // Массив нужно копировать поэлементно.
        if (valueType.IsArray)
        {
            Type elementType = valueType.GetElementType()!;

            // Сохраняем размер массива в локальную переменную.
            _il.Emit(OpCodes.Dup);
            LocalBuilder sizeLocal = SaveArraySizeToLocal();

            // Сохраняем исходный массив в локальную переменную.
            LocalBuilder srcValue = _il.DeclareLocal(valueType);
            _il.Emit(OpCodes.Stloc, srcValue);

            // Создаём новый массив и сохраняем в локальную переменную.
            LocalBuilder destValue = _il.DeclareLocal(valueType);
            _il.Emit(OpCodes.Ldloc, sizeLocal);
            _il.Emit(OpCodes.Newarr, valueType.GetElementType()!);
            _il.Emit(OpCodes.Stloc, destValue);

            // Копируем каждый элемент исходного массива в новый массив.
            EmitForEachIndex(sizeLocal, index =>
            {
                // Копируем значение из исходного массива в новый: destValue[index] := srcValue[index].
                _il.Emit(OpCodes.Ldloc, destValue);
                _il.Emit(OpCodes.Ldloc, index);
                _il.Emit(OpCodes.Ldloc, srcValue);
                _il.Emit(OpCodes.Ldloc, index);
                _il.Emit(OpCodes.Ldelem, elementType);
                EmitDeepCopy(elementType);
                _il.Emit(OpCodes.Stelem, elementType);
            });

            // Кладём на вершину стека ссылку на новый массив.
            _il.Emit(OpCodes.Ldloc, destValue);

            return;
        }

        throw new NotSupportedException($"Cannot deep copy value of type {valueType}");
    }

    /// <summary>
    /// Создаёт цикл для перебора индекса от 0 до size-1.
    /// </summary>
    /// <param name="size">Локальная переменная, которая хранит размер массива</param>
    /// <param name="action">Действие, принимает локальную переменную индекса массива.</param>
    private void EmitForEachIndex(LocalBuilder size, Action<LocalBuilder> action)
    {
        // Создаём анонимные локальные переменные для цикла обхода массива.
        LocalBuilder index = _il.DeclareLocal(typeof(int));

        // Инициализируем локальную переменную с индексом текущего элемента.
        _il.Emit(OpCodes.Ldc_I4, 0);
        _il.Emit(OpCodes.Stloc, index);

        Label loopStart = _il.DefineLabel(); // Метка проверки условия.
        Label loopEnd = _il.DefineLabel(); // Метка конца цикла.

        // Начало цикла: проверяем, не пора ли завершить заполнение массива.
        _il.MarkLabel(loopStart);
        _il.Emit(OpCodes.Ldloc, index);
        _il.Emit(OpCodes.Ldloc, size);
        _il.Emit(OpCodes.Clt);
        _il.Emit(OpCodes.Brfalse, loopEnd);

        // Вычисляем выражение и заполняем элемент массива.
        action(index);

        // Увеличиваем индекс на 1 и возвращаемся в начало цикла.
        _il.Emit(OpCodes.Ldloc, index);
        _il.Emit(OpCodes.Ldc_I4_1);
        _il.Emit(OpCodes.Add);
        _il.Emit(OpCodes.Stloc, index);
        _il.Emit(OpCodes.Br, loopStart);

        // Завершаем цикл инициализации.
        _il.MarkLabel(loopEnd);
    }

    /// <summary>
    /// Получает длину массива с текущей вершины стека и сохраняет в анонимную переменную типа int.
    /// </summary>
    private LocalBuilder SaveArraySizeToLocal()
    {
        LocalBuilder sizeLocal = _il.DeclareLocal(typeof(int));
        _il.Emit(OpCodes.Ldlen);
        _il.Emit(OpCodes.Conv_I4);
        _il.Emit(OpCodes.Stloc, sizeLocal);

        return sizeLocal;
    }

    /// <summary>
    /// Генерирует код вычисления бинарной операции над целыми числами.
    /// </summary>
    private void EmitIntegersBinaryOperation(BinaryOperationExpression e)
    {
        // Обрабатываем логические операции особым образом
        //  из-за вычисления по короткой схеме (short-circuit evaluation).
        switch (e.Operation)
        {
            case BinaryOperation.And:
                EmitLogicalAnd(e);
                return;
            case BinaryOperation.Or:
                EmitLogicalOr(e);
                return;
        }

        // Генерируем код для вычисления левого и правого операндов.
        e.Left.Accept(this);
        e.Right.Accept(this);

        // Генерируем инструкцию, соответствующую операции.
        switch (e.Operation)
        {
            case BinaryOperation.Add:
                _il.Emit(OpCodes.Add);
                break;
            case BinaryOperation.Subtract:
                _il.Emit(OpCodes.Sub);
                break;
            case BinaryOperation.Multiply:
                _il.Emit(OpCodes.Mul);
                break;
            case BinaryOperation.Divide:
                _il.Emit(OpCodes.Div);
                break;
            case BinaryOperation.Equal:
                _il.Emit(OpCodes.Ceq);
                break;
            case BinaryOperation.NotEqual:
                // Для реализации "<>" используем "==" и затем инвертируем результат (0 -> 1, 1 -> 0).
                _il.Emit(OpCodes.Ceq);
                EmitLogicalNot();
                break;
            case BinaryOperation.LessThan:
                _il.Emit(OpCodes.Clt);
                break;
            case BinaryOperation.LessThanOrEqual:
                // Для реализации "<=" используем ">" и затем инвертируем результат (0 -> 1, 1 -> 0).
                _il.Emit(OpCodes.Cgt);
                EmitLogicalNot();
                break;
            case BinaryOperation.GreaterThan:
                _il.Emit(OpCodes.Cgt);
                break;
            case BinaryOperation.GreaterThanOrEqual:
                // Для реализации ">=" используем "<" и затем инвертируем результат (0 -> 1, 1 -> 0).
                _il.Emit(OpCodes.Clt);
                EmitLogicalNot();
                break;
            default:
                throw new NotSupportedException($"Cannot generate MSIL for binary operation {e.Operation}.");
        }
    }

    /// <summary>
    /// Генерирует код логического "и" с вычислением по короткой схеме (short-circuit evaluation).
    /// </summary>
    private void EmitLogicalAnd(BinaryOperationExpression e)
    {
        Label falseLabel = _il.DefineLabel();
        Label endLabel = _il.DefineLabel();

        // Вычисляем левый операнд и сравниваем его с 0 ("ложь").
        // Если он равен 0, то всё выражение будет равно 0 ("ложь"), а правый операнд вычислять не надо.
        e.Left.Accept(this);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);
        _il.Emit(OpCodes.Brtrue, falseLabel);

        // Вычисляем правый и сравниваем его с 0 ("ложь").
        // Если он равен 0, то всё выражение будет равно 0 ("ложь"), иначе равно 1 ("истина").
        e.Right.Accept(this);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);
        _il.Emit(OpCodes.Brtrue, falseLabel);

        // Оба операнда — ненулевые числа ("истина"), поэтому результат выражения будет 1 ("истина").
        _il.Emit(OpCodes.Ldc_I4_1);
        _il.Emit(OpCodes.Br, endLabel);

        // Блок false: записываем 0 ("ложь") как результат выражения.
        _il.MarkLabel(falseLabel);
        _il.Emit(OpCodes.Ldc_I4_0);

        // Конец текущей последовательности инструкций.
        _il.MarkLabel(endLabel);
    }

    /// <summary>
    /// Генерирует код логического "или" с вычислением по короткой схеме (short-circuit evaluation).
    /// </summary>
    private void EmitLogicalOr(BinaryOperationExpression e)
    {
        Label trueLabel = _il.DefineLabel();
        Label endLabel = _il.DefineLabel();

        // Вычисляем левый операнд и сравниваем его с 0 ("ложь").
        // Если он не равен 0, то всё выражение будет равно 1 ("истина"), а правый операнд вычислять не надо.
        e.Left.Accept(this);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);
        _il.Emit(OpCodes.Brfalse, trueLabel);

        // Вычисляем правый и сравниваем его с 0 ("ложь").
        // Если он не равен 0, то всё выражение будет равно 1 ("истина"), иначе равно 0 ("ложь").
        e.Right.Accept(this);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);
        _il.Emit(OpCodes.Brfalse, trueLabel);

        // Оба операнда равны 0 ("ложь"), поэтому результат выражения будет 0 ("ложь").
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Br, endLabel);

        // Блок false: записываем 1 ("истина") как результат выражения.
        _il.MarkLabel(trueLabel);
        _il.Emit(OpCodes.Ldc_I4_1);

        // Конец текущей последовательности инструкций.
        _il.MarkLabel(endLabel);
    }

    /// <summary>
    /// Генерирует код бинарной операции сравнения строк.
    /// </summary>
    /// <remarks>
    /// В языке Tiger для строк поддерживаются только операции сравнения.
    /// Конкатенация, вычисление размера и разделение строки на части реализуются встроенными функциями.
    /// </remarks>
    private void EmitStringsBinaryOperation(BinaryOperationExpression e)
    {
        // Генерируем код для вычисления левого и правого операндов.
        e.Left.Accept(this);
        e.Right.Accept(this);

        // Метод string.CompareOrdinal(string, string) возвращает int:
        //   <0 если первая строка меньше второй,
        //   0 если равны,
        //   >0 если первая больше второй.
        MethodInfo compareOrdinal = GetMethod(typeof(string), "CompareOrdinal", [typeof(string), typeof(string)]);
        _il.Emit(OpCodes.Call, compareOrdinal);

        switch (e.Operation)
        {
            case BinaryOperation.Equal:
                // Строки равны, если результат CompareOrdinal равен 0.
                _il.Emit(OpCodes.Ldc_I4_0);
                _il.Emit(OpCodes.Ceq);
                break;
            case BinaryOperation.NotEqual:
                // Строки равны, если результат CompareOrdinal не равен 0,
                //  поэтому сравниваем на равенство с 0 и инвертируем результат (0 -> 1, 1 -> 0)
                _il.Emit(OpCodes.Ldc_I4_0);
                _il.Emit(OpCodes.Ceq);
                EmitLogicalNot();
                break;
            case BinaryOperation.LessThan:
                // Левая строка меньше правой, если результат CompareOrdinal меньше 0.
                _il.Emit(OpCodes.Ldc_I4_0);
                _il.Emit(OpCodes.Clt);
                break;
            case BinaryOperation.LessThanOrEqual:
                // Левая строка меньше или равна правой, если результат CompareOrdinal меньше или равен 0.
                _il.Emit(OpCodes.Ldc_I4_0);
                _il.Emit(OpCodes.Cgt);
                EmitLogicalNot();
                break;
            case BinaryOperation.GreaterThan:
                // Левая строка меньше правой, если результат CompareOrdinal больше 0.
                _il.Emit(OpCodes.Ldc_I4_0);
                _il.Emit(OpCodes.Cgt);
                break;
            case BinaryOperation.GreaterThanOrEqual:
                // Левая строка больше или равна правой, если результат CompareOrdinal больше или равен 0.
                _il.Emit(OpCodes.Ldc_I4_0);
                _il.Emit(OpCodes.Clt);
                EmitLogicalNot();
                break;
            default:
                throw new NotSupportedException($"Unexpected string binary operation {e.Operation}.");
        }
    }

    /// <summary>
    /// Выполняет логическое отрицание результата, аналогично функции `not(x: int)` в Tiger.
    /// Принимает любое число, возвращает 1 если оно было нулём, 0 в остальных случаях.
    /// </summary>
    private void EmitLogicalNot()
    {
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);
    }

    /// <summary>
    /// Находит статический метод указанного типа стандартной библиотеки классов .NET,
    ///  чтобы использовать его для реализации встроенной функции языка Tiger.
    /// </summary>
    private static MethodInfo GetMethod(Type type, string methodName, Type[] parameterTypes)
    {
        MethodInfo? method = type.GetMethod(methodName, parameterTypes);
        if (method == null)
        {
            string parameterTypeNames = string.Join(", ", parameterTypes.Select(t => t.Name));
            throw new InvalidOperationException($"Cannot find method {type.Name}.{methodName}({parameterTypeNames}).");
        }

        return method;
    }

    /// <summary>
    /// Находит статический метод указанного типа стандартной библиотеки классов .NET,
    ///  чтобы использовать его для реализации встроенной функции языка Tiger.
    /// </summary>
    private static MethodInfo GetMethod(Type type, string methodName, Func<MethodInfo, bool> predicate)
    {
        foreach (MethodInfo method in type.GetMethods())
        {
            if (method.Name == methodName && predicate(method))
            {
                return method;
            }
        }

        throw new InvalidOperationException($"Cannot find method {type.Name}.{methodName}(...).");
    }

    /// <summary>
    /// Добавляет новую область видимости переменных на стек.
    /// </summary>
    private void BeginScope()
    {
        _localVariables = new LocalVariablesScope(parent: _localVariables);
        _il.BeginScope();
    }

    /// <summary>
    /// Убирает текущую область видимости переменных со стека.
    /// </summary>
    private void EndScope()
    {
        _il.EndScope();
        _localVariables = _localVariables.Parent!;
    }

    private LocalBuilder EmitDefineVariable(string name, ValueType type, Expression initialValue)
    {
        // Объявляем локальную переменную нужного типа в текущем методе.
        LocalBuilder local = _il.DeclareLocal(_typeMapper.MapType(type));

        // Вычисляем начальное значение и сохраняем его в переменную.
        initialValue.Accept(this);
        _il.Emit(OpCodes.Stloc, local);

        // Добавляем переменную в текущую область видимости.
        _localVariables.AddVariable(name, local);

        return local;
    }

    /// <summary>
    /// Создает локальную переменную для i-го параметра функции (нумерация начинается с нуля).
    /// </summary>
    private void EmitDefineParameter(string name, ValueType type, int argumentNo)
    {
        // Создаём локальную переменную для параметра функции.
        LocalBuilder local = _il.DeclareLocal(_typeMapper.MapType(type));

        // Загружаем значение новой переменной из i-го аргумента (нумерация начинается с нуля).
        _il.Emit(OpCodes.Ldarg, argumentNo);
        _il.Emit(OpCodes.Stloc, local);

        // Добавляем в текущую область видимости.
        _localVariables.AddVariable(name, local);
    }

    /// <summary>
    /// Находит локальную переменную в текущей области видимости.
    /// </summary>
    private LocalBuilder FindVariable(string name)
    {
        return _localVariables.GetVariable(name);
    }

    /// <summary>
    /// Декорирует имя функции, чтобы гарантировать отсутствие пересечений с системными именами методов
    ///  (такими как "Main").
    /// </summary>
    private string GetUserFunctionMethodName(string name)
    {
        return "Tiger" + name;
    }

    /// <summary>
    /// Добавляет в класс Program метод с указанным именем, типами параметров и возвращаемым значением.
    /// </summary>
    private MethodBuilder DefineProgramClassMethod(string name, Type returnType, Type[] parameterTypes)
    {
        return _programTypeBuilder.DefineMethod(
            name,
            MethodAttributes.Public | MethodAttributes.Static,
            returnType,
            parameterTypes
        );
    }
}