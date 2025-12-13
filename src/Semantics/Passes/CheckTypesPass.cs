using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Semantics.Exceptions;

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
        CheckResultType("function body", d.Body, d.ResultType);
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
    }

    public override void Visit(WhileLoopExpression e)
    {
        base.Visit(e);

        CheckResultType("while loop condition", e.Condition, ValueType.Int);
        CheckResultType("while loop body", e.LoopBody, ValueType.Void);
    }

    public override void Visit(ForLoopExpression e)
    {
        base.Visit(e);

        CheckResultType("for loop start value", e.StartValue, ValueType.Int);
        CheckResultType("for loop end value", e.EndValue, ValueType.Int);
        CheckResultType("for loop body", e.LoopBody, ValueType.Void);
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