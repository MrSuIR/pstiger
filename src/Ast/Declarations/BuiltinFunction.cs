using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Определение встроенной функции языка.
/// </summary>
public sealed class BuiltinFunction : AbstractFunctionDeclaration
{
    public BuiltinFunction(
        string name,
        IReadOnlyList<BuiltinFunctionParameter> parameters,
        ValueType resultType
    )
        : base(name, parameters)
    {
        ResultType = resultType;
    }

    public override void Accept(IAstVisitor visitor)
    {
        throw new InvalidOperationException($"Visitor cannot be applied to {GetType()}");
    }
}