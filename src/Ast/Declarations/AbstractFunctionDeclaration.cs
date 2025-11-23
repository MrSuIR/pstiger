using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

public abstract class AbstractFunctionDeclaration : Declaration
{
    protected AbstractFunctionDeclaration(
        string name,
        IReadOnlyList<ParameterDeclaration> parameters,
        ValueType resultType
    )
    {
        Name = name;
        Parameters = parameters;
        ResultType = resultType;
    }

    public string Name { get; }

    public IReadOnlyList<ParameterDeclaration> Parameters { get; }
}