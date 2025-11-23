using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

public abstract class AbstractTypeDeclaration : Declaration
{
    protected AbstractTypeDeclaration(string name, ValueType type)
    {
        Name = name;
        ResultType = type;
    }

    public string Name { get; }
}