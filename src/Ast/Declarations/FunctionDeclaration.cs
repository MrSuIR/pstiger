using PsTiger.Ast.Expressions;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Объявление пользовательской функции или процедуры.
/// </summary>
public sealed class FunctionDeclaration : AbstractFunctionDeclaration
{
    public FunctionDeclaration(
        string name,
        IReadOnlyList<ParameterDeclaration> parameters,
        string? resultTypeName,
        Expression body
    )
        : base(name, parameters)
    {
        ResultTypeName = resultTypeName;
        Body = body;
    }

    public string? ResultTypeName { get; }

    public Expression Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}