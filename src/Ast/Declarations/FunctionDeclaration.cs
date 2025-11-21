using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Объявление функции языка.
/// </summary>
public abstract class FunctionDeclaration
{
    protected FunctionDeclaration(string name, IReadOnlyList<ParameterDeclaration> parameters, ValueType resultType)
    {
        Name = name;
        Parameters = parameters;
        ResultType = resultType;
    }

    public string Name { get; }

    public IReadOnlyList<ParameterDeclaration> Parameters { get; }

    public ValueType ResultType { get; }
}