using PsTiger.Ast.Attributes;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Expressions;

/// <summary>
/// Абстрактный подкласс выражения.
/// </summary>
public abstract class Expression : AstNode
{
    private AstAttribute<ValueType> _resultType;

    /// <summary>
    /// Тип результата выражения.
    /// </summary>
    public ValueType ResultType
    {
        get => _resultType.Get();

        set => _resultType.Set(value);
    }
}