using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

public abstract class AbstractTypeDeclaration : Declaration
{
    protected AbstractTypeDeclaration(string name)
    {
        Name = name;
    }

    public string Name { get; }
}