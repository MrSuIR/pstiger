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
        IReadOnlyList<BuiltinFunctionParameter> parameters,
        ValueType resultType,
        Func<IReadOnlyList<Value>, Value> implementation
    )
        : base(name, parameters)
    {
        ResultType = resultType;
        _implementation = implementation;
    }

    public Value Invoke(IReadOnlyList<Value> arguments)
    {
        return _implementation(arguments);
    }

    public override void Accept(IAstVisitor visitor)
    {
        throw new InvalidOperationException($"Visitor cannot be applied to {GetType()}");
    }
}