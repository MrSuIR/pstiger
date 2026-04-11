using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Runtime;
using PsTiger.Semantics.Exceptions;
using PsTiger.Semantics.Helpers;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Semantics.Passes;

/// <summary>
/// Проход по AST для проверки корректности программы с точки зрения совместимости типов данных.
/// </summary>
/// <exception cref="TypeErrorException">Бросается при несоответствии типов данных в процессе проверки.</exception>
public class CheckTypesPass : AbstractPass
{
    /// <summary>
    /// Проверяет соответствие типов параметров функции и аргументов при вызове этой функции.
    /// </summary>
    public override void Visit(FunctionCallExpression e)
    {
        base.Visit(e);
        CheckFunctionArgumentTypes(e, e.Function);
    }

    public override void Visit(FunctionDeclaration d)
    {
        base.Visit(d);
        CheckAreSameTypes("function body", d.Body, d.ResultType);
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

        if (d.DeclaredType != null && !ValueTypeUtil.AreCompatibleTypes(d.DeclaredType.ResultType, inferredType))
        {
            throw new TypeErrorException(
                $"Cannot initialize variable of type {d.DeclaredTypeName} with value of type {inferredType}"
            );
        }

        if (d.DeclaredType == null && inferredType == ValueType.Nil)
        {
            throw new TypeErrorException(
                $"Variable {d.Name} type cannot be inferred from nil"
            );
        }
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);
        if (!ValueTypeUtil.AreCompatibleTypes(e.Left.ResultType, e.Right.ResultType))
        {
            throw new TypeErrorException(
                $"Cannot assign value of type {e.Right.ResultType} to variable of type {e.Left.ResultType}"
            );
        }
    }

    public override void Visit(IfElseExpression e)
    {
        base.Visit(e);

        CheckAreSameTypes("if-else condition", e.Condition, ValueType.Int);

        ValueType thenType = e.ThenBranch.ResultType;
        if (e.ElseBranch != null)
        {
            CheckAreCompatibleTypes("else branch", e.ElseBranch, thenType);
        }
        else if (thenType != ValueType.Void)
        {
            throw new TypeErrorException("The \"if...then\" expression without \"else\" branch may not return value");
        }
    }

    public override void Visit(WhileLoopExpression e)
    {
        base.Visit(e);

        CheckAreSameTypes("while loop condition", e.Condition, ValueType.Int);
        CheckAreSameTypes("while loop body", e.LoopBody, ValueType.Void);
    }

    public override void Visit(ForLoopExpression e)
    {
        base.Visit(e);

        CheckAreSameTypes("for loop start value", e.StartValue, ValueType.Int);
        CheckAreSameTypes("for loop end value", e.EndValue, ValueType.Int);
        CheckAreSameTypes("for loop body", e.LoopBody, ValueType.Void);
    }

    public override void Visit(ArrayAccessExpression e)
    {
        base.Visit(e);

        CheckAreSameTypes("array index", e.Index, ValueType.Int);
    }

    public override void Visit(ArrayLiteralExpression e)
    {
        base.Visit(e);

        ValueType elementType = e.ResultType switch
        {
            ArrayType arrayType => arrayType.ElementType,
            _ => throw new TypeErrorException($"Unexpected non-array value type {e.ResultType}"),
        };

        CheckAreCompatibleTypes("array initialization value", e.InitialValue, elementType);
    }

    public override void Visit(RecordLiteralExpression e)
    {
        base.Visit(e);

        if (e.RecordType.ResultType is not RecordType recordType)
        {
            throw InvalidRecordLiteralException.ExpectedRecordType(e.RecordTypeName, e.RecordType.ResultType);
        }

        Dictionary<string, int> fieldIndexes = GetRecordFieldIndexes(recordType);

        int initializerIndex = 0;
        foreach (FieldInitializer initializer in e.Initializers)
        {
            if (!recordType.Fields.TryGetValue(initializer.Name, out ValueType? fieldType))
            {
                throw InvalidRecordLiteralException.UnexpectedFieldName(initializer.Name, recordType);
            }

            CheckAreCompatibleTypes("field initializer", initializer.Value, fieldType);

            int fieldIndex = fieldIndexes[initializer.Name];
            if (initializerIndex != fieldIndex)
            {
                throw InvalidRecordLiteralException.WrongFieldInitializerIndex(
                    initializer.Name, fieldIndex, initializerIndex
                );
            }

            ++initializerIndex;
        }

        if (e.Initializers.Count < recordType.Fields.Count)
        {
            string missingField = recordType.Fields.Keys.Except(e.Initializers.Select(f => f.Name)).First();
            throw InvalidRecordLiteralException.MissingFieldInitializer(missingField, e.RecordTypeName);
        }
    }

    private static Dictionary<string, int> GetRecordFieldIndexes(RecordType recordType)
    {
        Dictionary<string, int> fieldIndexes = [];
        int index = 0;
        foreach (string name in recordType.Fields.Keys)
        {
            fieldIndexes[name] = index;
            ++index;
        }

        return fieldIndexes;
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
            if (!ValueTypeUtil.AreCompatibleTypes(argument.ResultType, parameter.ResultType))
            {
                throw new TypeErrorException(
                    $"Cannot apply argument #{i} of type {argument.ResultType} to function {e.Name} parameter {parameter.Name} which has type {parameter.ResultType}"
                );
            }
        }
    }

    private static void CheckAreSameTypes(string category, Expression expression, ValueType expectedType)
    {
        if (!ValueTypeUtil.AreCompatibleTypes(expression.ResultType, expectedType))
        {
            throw new TypeErrorException(category, expectedType, expression.ResultType);
        }
    }

    private static void CheckAreCompatibleTypes(string category, Expression expression, ValueType expectedType)
    {
        if (!ValueTypeUtil.AreCompatibleTypes(expression.ResultType, expectedType))
        {
            throw new TypeErrorException(category, expectedType, expression.ResultType);
        }
    }
}