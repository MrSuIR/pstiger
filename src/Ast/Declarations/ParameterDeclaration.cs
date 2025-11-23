using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Объявление параметра функции.
/// </summary>
public class ParameterDeclaration : AbstractParameterDeclaration
{
    public ParameterDeclaration(string name, string typeName)
        : base(name)
    {
        this.TypeName = typeName;
    }

    public string TypeName { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}