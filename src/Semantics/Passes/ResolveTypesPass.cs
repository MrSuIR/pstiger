using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Semantics.Passes;

/// <summary>
/// Проход по AST для вычисления типов данных.
/// </summary>
/// <exception cref="TypeErrorException">Бросается при несоответствии типов данных в процессе вычисления типов.</exception>
public sealed class ResolveTypesPass : AbstractPass
{
    /// <summary>
    /// Литерал всегда имеет определённый тип.
    /// </summary>
    public override void Visit(LiteralExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Type;
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
        if (resultType is null)
        {
            throw new TypeErrorException(
                $"Binary operation {e.Operation} is not allowed for types {e.Left.ResultType} and {e.Right.ResultType}"
            );
        }

        e.ResultType = resultType;
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

    public override void Visit(FunctionCallExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Function.ResultType;
    }

    public override void Visit(ScopeExpression e)
    {
        // NOTE: Для поддержки взаимной рекурсии функций мы выполняем обход дочерних узлов необычным способом:
        // 1. Для подряд идущих объявлений функций мы обрабатываем их заранее (до посещения дочерних узлов)
        // 2. Как только подряд идущие функции заканчиваются — запускаем обход узлов этих функций.
        Queue<Declaration> visitQueue = [];

        // Обходим объявления, при этом идущие подряд функции объявляем заранее.
        foreach (Declaration d in e.Declarations)
        {
            if (d is FunctionDeclaration f)
            {
                // Заранее сохраняем тип функции.
                f.ResultType = f.DeclaredType?.ResultType ?? ValueType.Void;
                visitQueue.Enqueue(d);
            }
            else
            {
                ProcessVisitQueue();
                d.Accept(this);
            }
        }

        ProcessVisitQueue();

        // Обходим последовательность выражений в данной области видимости.
        foreach (Expression nested in e.Expressions)
        {
            nested.Accept(this);
        }

        // Выражение var...in...end возвращает результат последнего из последовательности вложенных выражений.
        if (e.Expressions.Count > 0)
        {
            e.ResultType = e.Expressions[^1].ResultType;
        }
        else
        {
            e.ResultType = ValueType.Void;
        }

        return;

        void ProcessVisitQueue()
        {
            while (visitQueue.TryDequeue(out Declaration? declaration))
            {
                declaration.Accept(this);
            }
        }
    }

    public override void Visit(ParameterDeclaration d)
    {
        d.ResultType = d.Type.ResultType;
    }

    public override void Visit(VariableAccessExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Variable.ResultType;
    }

    public override void Visit(VariableDeclaration d)
    {
        base.Visit(d);
        d.ResultType = d.InitialValue.ResultType;
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);
        e.ResultType = ValueType.Void;
    }

    public override void Visit(IfElseExpression e)
    {
        base.Visit(e);
        e.ResultType = e.ThenBranch.ResultType;
    }

    public override void Visit(WhileLoopExpression e)
    {
        base.Visit(e);

        e.ResultType = ValueType.Void;
    }

    public override void Visit(ForLoopExpression e)
    {
        base.Visit(e);
        e.ResultType = ValueType.Void;
    }

    public override void Visit(ForIteratorDeclaration d)
    {
        base.Visit(d);
        d.ResultType = ValueType.Int;
    }

    public override void Visit(BreakLoopExpression e)
    {
        base.Visit(e);
        e.ResultType = ValueType.Void;
    }

    public override void Visit(TypeDeclaration d)
    {
        base.Visit(d);

        d.ResultType = d.TypeExpression switch
        {
            NamedTypeExpression namedType => namedType.Type.ResultType,
            ArrayTypeExpression arrayType => new ArrayType(arrayType.ElementType.ResultType),
            _ => throw new InvalidOperationException($"Unexpected type expression class {d.TypeExpression.GetType()}"),
        };
    }

    public override void Visit(ArrayAccessExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Array.ResultType switch
        {
            ArrayType arrayType => arrayType.ElementType,
            _ => throw new TypeErrorException($"Cannot use type {e.Array.ResultType} as array"),
        };
    }

    public override void Visit(ArrayLiteralExpression e)
    {
        base.Visit(e);
        e.ResultType = e.ArrayType.ResultType;
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
                if (left == ValueType.Int && right == ValueType.Int)
                {
                    return ValueType.Int;
                }

                if (left == ValueType.String && right == ValueType.String)
                {
                    return ValueType.Int;
                }

                return null;

            case BinaryOperation.Equal:
            case BinaryOperation.NotEqual:
                if (left == right)
                {
                    return ValueType.Int;
                }

                return null;

            default:
                throw new InvalidOperationException($"Unknown binary operation {operation}");
        }
    }
}