using PsTiger.Ast.Attributes;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

public class FieldDeclaration : Declaration
{
    private AstAttribute<AbstractTypeDeclaration> _type;

    public FieldDeclaration(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    public string Name { get; set; }

    public string TypeName { get; set; }

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