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

    // Стек областей видимости переменных.
    private readonly Stack<Dictionary<string, LocalBuilder>> _scopesStack;

    public MsilCodegenPass(ModuleBuilder moduleBuilder)
    {
        _moduleBuilder = moduleBuilder;
        _typeMapper = new TigerTypeMapper();
        _builtinFunctionEmitter = new BuiltinFunctionEmitter();
        _scopesStack = new Stack<Dictionary<string, LocalBuilder>>();
    }

    /// <summary>
    /// Текущая область видимости переменных.
    /// </summary>
    private Dictionary<string, LocalBuilder> CurrentScope => _scopesStack.Peek();

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

        if (e.Function is BuiltinFunction)
        {
            _builtinFunctionEmitter.EmitCallBuiltinFunction(e.Name, _il);
            return;
        }

        throw new NotImplementedException("User functions are not supported yet");
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
        // Генерируем код для правой части присваивания.
        e.Right.Accept(this);

        if (e.Left is VariableAccessExpression variableAccess)
        {
            // Сохраняем вычисленное выражение в переменной.
            LocalBuilder local = FindVariable(variableAccess.Variable.Name);
            _il.Emit(OpCodes.Stloc, local);
        }
        else
        {
            throw new NotImplementedException($"Assignment to {e.Left.GetType()} lvalue is not implemented yet");
        }
    }

    public void Visit(IfElseExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(VariableDeclaration d)
    {
        // Объявляем локальную переменную нужного типа в текущем методе.
        Type type = _typeMapper.MapType(d.InitialValue.ResultType);
        LocalBuilder local = _il.DeclareLocal(type);

        // Вычисляем начальное значение и сохраняем его в переменную.
        d.InitialValue.Accept(this);
        _il.Emit(OpCodes.Stloc, local);

        // Добавляем переменную в текущую область видимости.
        CurrentScope[d.Name] = local;
    }

    public void Visit(FunctionDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(ParameterDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(WhileLoopExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ForLoopExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ForIteratorDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(BreakLoopExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(TypeDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(NamedTypeExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ArrayTypeExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ArrayAccessExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ArrayLiteralExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(RecordTypeExpression e)
    {
        throw new NotImplementedException();
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
    /// Добавляет новую область видимости переменных на стек.
    /// </summary>
    private void BeginScope()
    {
        _scopesStack.Push(new Dictionary<string, LocalBuilder>());
        _il.BeginScope();
    }

    /// <summary>
    /// Убирает текущую область видимости переменных со стека.
    /// </summary>
    private void EndScope()
    {
        _il.EndScope();
        _scopesStack.Pop();
    }

    /// <summary>
    /// Находит локальную переменную по имени, просматривая лексические области видимости от текущей к внешним.
    /// </summary>
    private LocalBuilder FindVariable(string name)
    {
        // Ищем переменную в текущей и родительских областях видимости.
        // В языке C# инструкция foreach для класса Stack выбирает значения, начиная с последнего добавленного.
        foreach (Dictionary<string, LocalBuilder> scope in _scopesStack)
        {
            if (scope.TryGetValue(name, out LocalBuilder? local))
            {
                return local;
            }
        }

        throw new InvalidOperationException($"Variable '{name}' not found in current scopes.");
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