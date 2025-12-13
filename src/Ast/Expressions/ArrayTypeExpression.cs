using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

/// <summary>
/// Выражение, определяющее тип массива с заданным типом элементов: "array of T".
/// </summary>
public sealed class ArrayTypeExpression : AbstractTypeExpression
{
    private AstAttribute<AbstractTypeDeclaration> _elementType;

    public ArrayTypeExpression(string elementTypeName)
    {
        ElementTypeName = elementTypeName;
    }

    /// <summary>
    /// Имя типа элемента массива.
    /// </summary>
    public string ElementTypeName { get; }

    /// <summary>
    /// Тип элемента массива.
    /// </summary>
    public AbstractTypeDeclaration ElementType
    {
        get => _elementType.Get();
        set => _elementType.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}