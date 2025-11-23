using PsTiger.Ast.Attributes;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Объявление параметра функции.
/// </summary>
public class ParameterDeclaration : AbstractParameterDeclaration
{
    private AstAttribute<AbstractTypeDeclaration?> _declaredType;

    public ParameterDeclaration(string name, string typeName)
        : base(name)
    {
        this.TypeName = typeName;
    }

    public string TypeName { get; }

    public AbstractTypeDeclaration Type
    {
        get => _declaredType.Get() ?? throw new InvalidOperationException(
            $"No declaration for parameter type {this.TypeName}"
        );
        set => _declaredType.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}