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

    public MsilCodegenPass(ModuleBuilder moduleBuilder)
    {
        _moduleBuilder = moduleBuilder;
        _typeMapper = new TigerTypeMapper();
        _builtinFunctionEmitter = new BuiltinFunctionEmitter();
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

        program.Accept(this);

        // В Tiger программа может быть выражением, возвращающим значение.
        // В .NET функция Main должна иметь тип void.
        if (program.ResultType != ValueType.Void)
        {
            _il.Emit(OpCodes.Pop);
        }

        // Завершаем метод Main инструкцией ret.
        _il.Emit(OpCodes.Ret);

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
        for (int i = 0; i < e.Sequence.Count; i++)
        {
            Expression expr = e.Sequence[i];
            expr.Accept(this);

            // Если это не последнее выражение и его тип не void, отбрасываем результат.
            if (i != e.Sequence.Count - 1 && expr.ResultType != ValueType.Void)
            {
                _il?.Emit(OpCodes.Pop);
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
        throw new NotImplementedException();
    }

    public void Visit(VariableAccessExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(AssignmentExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(IfElseExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(VariableDeclaration d)
    {
        throw new NotImplementedException();
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