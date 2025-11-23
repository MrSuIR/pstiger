using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Определение встроенной функции языка.
/// </summary>
public sealed class BuiltinFunction : AbstractFunctionDeclaration
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

    public override void Accept(IAstVisitor visitor)
    {
        throw new NotImplementedException($"Visitor cannot be applied to {GetType()}");
    }
}