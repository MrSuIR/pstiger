using PsTiger.Ast.Attributes;
using PsTiger.Ast.Expressions;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Объявление пользовательской функции или процедуры.
/// </summary>
public sealed class FunctionDeclaration : AbstractFunctionDeclaration
{
    private AstAttribute<AbstractTypeDeclaration?> _declaredType;

    public FunctionDeclaration(
        string name,
        IReadOnlyList<ParameterDeclaration> parameters,
        string? declaredTypeName,
        Expression body
    )
        : base(name, parameters)
    {
        DeclaredTypeName = declaredTypeName;
        Body = body;
    }

    public string? DeclaredTypeName { get; }

    public AbstractTypeDeclaration? DeclaredType
    {
        get => _declaredType.Get();
        set => _declaredType.Set(value);
    }

    public Expression Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}