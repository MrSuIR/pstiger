using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

/// <summary>
/// Выражение, указывающее на имя другого типа данных.
/// </summary>
public sealed class NamedTypeExpression : AbstractTypeExpression
{
    private AstAttribute<AbstractTypeDeclaration> _type;

    public NamedTypeExpression(string typeName)
    {
        TypeName = typeName;
    }

    /// <summary>
    /// Имя типа, на который ссылается выражение.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Тип, на который ссылается выражение.
    /// </summary>
    public AbstractTypeDeclaration Type
    {
        get => _type.Get();

        set => _type.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}