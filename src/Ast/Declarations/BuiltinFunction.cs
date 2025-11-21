using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Определение встроенной функции языка.
/// </summary>
public class BuiltinFunction : FunctionDeclaration
{
    private readonly Func<IReadOnlyList<Value>, Value> _implementation;

    public BuiltinFunction(
        string name,
        IReadOnlyList<ParameterDeclaration> parameters,
        ValueType resultType,
        Func<IReadOnlyList<Value>, Value> implementation
    )
        : base(name, parameters, resultType)
    {
        _implementation = implementation;
    }

    public Value Invoke(IReadOnlyList<Value> arguments)
    {
        return _implementation(arguments);
    }
}