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
        // NOTE: Для поддержки взаимной рекурсии функций мы выполняем обход функций до основной части обхода.
        foreach (Declaration d in e.Declarations)
        {
            if (d is FunctionDeclaration f)
            {
                f.ResultType = f.DeclaredType?.ResultType ?? ValueType.Void;
                foreach (ParameterDeclaration p in f.Parameters.Cast<ParameterDeclaration>())
                {
                    p.ResultType = p.Type.ResultType;
                }
            }
        }

        base.Visit(e);
        e.ResultType = ValueType.Void;
    }

    public override void Visit(VariableAccessExpression e)
    {
        base.Visit(e);

        e.ResultType = e.Variable.ResultType;
    }

    /// <summary>
    /// Проверяет тип переменной и тип выражения, которым она инициализируется.
    /// </summary>
    public override void Visit(VariableDeclaration d)
    {
        base.Visit(d);

        ValueType inferredType = d.InitialValue.ResultType;
        if (inferredType == ValueType.Void)
        {
            throw new TypeErrorException("Cannot initialize variable from expression without value");
        }

        if (d.DeclaredType != null && d.DeclaredType.ResultType != inferredType)
        {
            throw new TypeErrorException(
                $"Cannot initialize variable of type {d.DeclaredTypeName} with value of type {inferredType}"
            );
        }

        d.ResultType = inferredType;
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

        e.ResultType = ValueType.Void;
    }

    public override void Visit(IfElseExpression e)
    {
        base.Visit(e);

        CheckResultType("if-else condition", e.Condition, ValueType.Int);

        ValueType thenType = e.ThenBranch.ResultType;

        if (e.ElseBranch != null)
        {
            CheckResultType("else branch", e.ElseBranch, thenType);
        }
        else if (thenType != ValueType.Void)
        {
            throw new TypeErrorException("The \"if...then\" expression without \"else\" branch may not return value");
        }

        e.ResultType = thenType;
    }

    public override void Visit(FunctionDeclaration d)
    {
        base.Visit(d);

        CheckResultType("function body", d.Body, d.ResultType);
    }

    public override void Visit(WhileLoopExpression e)
    {
        base.Visit(e);

        CheckResultType("while loop condition", e.Condition, ValueType.Int);
        CheckResultType("while loop body", e.LoopBody, ValueType.Void);
        e.ResultType = ValueType.Void;
    }

    public override void Visit(ForLoopExpression e)
    {
        base.Visit(e);

        CheckResultType("for loop start value", e.StartValue, ValueType.Int);
        CheckResultType("for loop end value", e.EndValue, ValueType.Int);
        CheckResultType("for loop body", e.LoopBody, ValueType.Void);
        e.ResultType = ValueType.Void;
    }

    public override void Visit(ForIteratorDeclaration d)
    {
        base.Visit(d);
        d.ResultType = ValueType.Int;
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
    private static void CheckFunctionArgumentTypes(FunctionCallExpression e, AbstractFunctionDeclaration function)
    {
        // Для каждого i-го аргумента выводим тип и сверяем с типом i-го параметра функции.
        for (int i = 0, iMax = e.Arguments.Count; i < iMax; ++i)
        {
            Expression argument = e.Arguments[i];
            AbstractParameterDeclaration parameter = function.Parameters[i];
            if (argument.ResultType != parameter.ResultType)
            {
                throw new TypeErrorException(
                    $"Cannot apply argument #{i} of type {argument.ResultType} to function {e.Name} parameter {parameter.Name} which has type {parameter.ResultType}"
                );
            }
        }
    }

    private static void CheckResultType(string category, Expression expression, ValueType expectedType)
    {
        if (expression.ResultType != expectedType)
        {
            throw new TypeErrorException(category, expectedType, expression.ResultType);
        }
    }
}