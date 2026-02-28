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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public void Visit(FunctionCallExpression e)
    {
        // Генерируем код для аргументов (они должны быть добавлены на стек в порядке перечисления).
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }

        if (_builtinFunctionEmitter.IsBuiltinFunction(e.Name))
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