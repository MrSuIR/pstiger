using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Semantics.Exceptions;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Semantics.Passes;

/// <summary>
/// Проход по AST выполняет две задачи:
///  1. Вычислить типы данных.
///  2. Проверить корректность программы с точки зрения совместимости типов данных.
/// </summary>
/// <exception cref="TypeErrorException">Бросается при несоответствии типов данных.</exception>
public sealed class ResolveTypesPass : AbstractPass
{
    /// <summary>
    /// Литерал всегда имеет определённый тип.
    /// </summary>
    public override void Visit(LiteralExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Value.GetValueType();
    }

    /// <summary>
    /// Выполняет проверки типов для бинарных операций:
    /// 1. Арифметические и логические операции выполняются над целыми числами и возвращают число.
    /// 2. Операции сравнения выполняются над двумя числами либо двумя строками и возвращают тот же тип.
    /// </summary>
    public override void Visit(BinaryOperationExpression e)
    {
        base.Visit(e);

        ValueType? resultType = GetBinaryOperationResultType(e.Operation, e.Left.ResultType, e.Right.ResultType);
        if (resultType == null)
        {
            throw new TypeErrorException(
                $"Binary operation {e.Operation} is not allowed for types {e.Left.ResultType} and {e.Right.ResultType}"
            );
        }

        e.ResultType = resultType.Value;
    }

    /// <summary>
    /// Выполняет проверки типов для последовательности выражений:
    ///  1. Пустая последовательность `()` не возвращает значения.
    ///  2. Непустая последовательность возвращает результат последнего выражения.
    ///  3. Все выражения в последовательности должны быть соблюдать семантику языка.
    /// </summary>
    public override void Visit(SequenceExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Sequence.Count > 0 ? e.Sequence[^1].ResultType : ValueType.Void;
    }

    /// <summary>
    /// Выполняет проверки типов для унарного минуса.
    /// Унарный минус применяется только к целым числам и возвращает целое число.
    /// </summary>
    public override void Visit(UnaryMinusExpression e)
    {
        base.Visit(e);

        ValueType operandType = e.Operand.ResultType;
        if (operandType != ValueType.Int)
        {
            throw new TypeErrorException($"Unary minus operation is not allowed for type {operandType}");
        }

        e.ResultType = operandType;
    }

    /// <summary>
    /// Проверяет соответствие типов параметров функции и аргументов при вызове этой функции.
    /// </summary>
    public override void Visit(FunctionCallExpression e)
    {
        base.Visit(e);

        CheckFunctionArgumentTypes(e, e.Function);
        e.ResultType = e.Function.ResultType;
    }

    /// <summary>
    /// Выражение var...in...end не возвращает результата.
    /// </summary>
    public override void Visit(ScopeExpression e)
    {
        base.Visit(e);
        e.ResultType = ValueType.Void;
    }

    /// <summary>
    /// Проверяет тип переменной и тип выражения, которым она инициализируется.
    /// </summary>
    public override void Visit(VariableDeclaration e)
    {
        base.Visit(e);

        ValueType inferredType = e.InitialValue.ResultType;
        if (inferredType == ValueType.Void)
        {
            throw new TypeErrorException("Cannot initialize variable from expression without value");
        }

        if (e.DeclaredType != inferredType)
        {
            throw new TypeErrorException(
                $"Cannot initialize variable of type {e.DeclaredType} with value of type {inferredType}"
            );
        }
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);

        if (e.Left.ResultType != e.Right.ResultType)
        {
            throw new TypeErrorException(
                $"Cannot assign value of type {e.Right.ResultType} to variable of type {e.Left.ResultType}"
            );
        }
    }

    /// <summary>
    /// Вычисляет тип результата бинарной операции.
    /// Возвращает null, если бинарная операция не может быть выполнена с указанными типами.
    /// </summary>
    private static ValueType? GetBinaryOperationResultType(BinaryOperation operation, ValueType left, ValueType right)
    {
        switch (operation)
        {
            case BinaryOperation.Add:
            case BinaryOperation.Substract:
            case BinaryOperation.Multiply:
            case BinaryOperation.Divide:
            case BinaryOperation.Or:
            case BinaryOperation.And:
                if (left == ValueType.Int && right == ValueType.Int)
                {
                    return ValueType.Int;
                }

                return null;
            case BinaryOperation.LessThan:
            case BinaryOperation.GreaterThan:
            case BinaryOperation.LessThanOrEqual:
            case BinaryOperation.GreaterThanOrEqual:
            case BinaryOperation.Equal:
            case BinaryOperation.NotEqual:
                if (left == ValueType.Int && right == ValueType.Int)
                {
                    return ValueType.Int;
                }

                if (left == ValueType.String && right == ValueType.String)
                {
                    return ValueType.String;
                }

                return null;

            default:
                throw new ArgumentException($"Unknown binary operation {operation}");
        }
    }

    /// <summary>
    /// Проверяет соответствие типов формальных параметров и фактических параметров (аргументов) при вызове функции.
    /// </summary>
    private void CheckFunctionArgumentTypes(FunctionCallExpression e, AbstractFunctionDeclaration function)
    {
        // Для каждого i-го аргумента выводим тип и сверяем с типом i-го параметра функции.
        for (int i = 0, iMax = e.Arguments.Count; i < iMax; ++i)
        {
            Expression argument = e.Arguments[i];
            ParameterDeclaration parameter = function.Parameters[i];
            if (argument.ResultType != parameter.ValueType)
            {
                throw new TypeErrorException(
                    $"Cannot apply argument #{i} of type {argument.ResultType} to function {e.Name} parameter {parameter.Name} which has type {parameter.ValueType}"
                );
            }
        }
    }
}