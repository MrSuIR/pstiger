using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Semantics.Helpers;

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
        DeclarationVisitQueue visitQueue = new(this);

        // Обходим объявления, при этом идущие подряд функции объявляем заранее.
        foreach (Declaration d in e.Declarations)
        {
            switch (d)
            {
                case FunctionDeclaration f:
                    // Заранее сохраняем тип функции.
                    visitQueue.BeforeFunctionDeclaration();
                    f.ResultType = f.DeclaredType?.ResultType ?? ValueType.Void;
                    visitQueue.Enqueue(d);
                    break;

                case TypeDeclaration t:
                    // 1. Для структур и массивов заранее заявляем тип
                    // 2. Добавляем объявление типа в очередь обхода.
                    visitQueue.BeforeTypeDeclaration();
                    switch (t.TypeExpression)
                    {
                        case RecordTypeExpression:
                            t.ResultType = new RecordType();
                            break;

                        case ArrayTypeExpression:
                            t.ResultType = new ArrayType();
                            break;
                    }

                    visitQueue.Enqueue(d);
                    break;

                default:
                    visitQueue.Flush();
                    d.Accept(this);
                    break;
            }
        }

        visitQueue.Flush();

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

        switch (d.TypeExpression)
        {
            case NamedTypeExpression namedTypeExpression:
                d.ResultType = namedTypeExpression.Type.ResultType;
                break;

            case RecordTypeExpression recordTypeExpression:
                RecordType recordType = (RecordType)d.ResultType;
                recordType.Fields = ResolveRecordFields(recordTypeExpression.Fields);
                break;

            case ArrayTypeExpression arrayTypeExpression:
                ArrayType arrayType = (ArrayType)d.ResultType;
                arrayType.ElementType = arrayTypeExpression.ElementType.ResultType;
                break;
        }
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

    public override void Visit(FieldAccessExpression e)
    {
        base.Visit(e);

        // Определяем тип всего выражения по типу структуры и названию поля.
        if (e.Record.ResultType is RecordType recordType)
        {
            if (recordType.Fields.TryGetValue(e.FieldName, out ValueType? fieldType))
            {
                e.ResultType = fieldType;
            }
            else
            {
                throw new TypeErrorException($"Field \"{e.FieldName}\" does not exist in {e.Record}");
            }
        }
        else
        {
            throw new TypeErrorException($"Cannot use type {e.Record.ResultType} as record");
        }
    }

    public override void Visit(ArrayLiteralExpression e)
    {
        base.Visit(e);
        e.ResultType = e.ArrayType.ResultType;
    }

    public override void Visit(RecordLiteralExpression e)
    {
        base.Visit(e);
        e.ResultType = e.RecordType.ResultType;
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
            case BinaryOperation.Subtract:
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
                if (ValueTypeUtil.AreCompatibleTypes(left, right) && !(left == ValueType.Nil && right == ValueType.Nil))
                {
                    return ValueType.Int;
                }

                return null;

            default:
                throw new InvalidOperationException($"Unknown binary operation {operation}");
        }
    }

    private static Dictionary<string, ValueType> ResolveRecordFields(
        Dictionary<string, AbstractTypeDeclaration> fieldTypeDeclarations
    )
    {
        Dictionary<string, ValueType> fields = [];
        foreach ((string name, AbstractTypeDeclaration declaration) in fieldTypeDeclarations)
        {
            fields[name] = declaration.ResultType;
        }

        return fields;
    }
}