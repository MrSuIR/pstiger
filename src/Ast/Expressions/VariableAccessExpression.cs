using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

/// <summary>
/// Выражение доступа к переменной по имени.
/// </summary>
public sealed class VariableAccessExpression : Expression
{
    private AstAttribute<VariableDeclaration> _variable;

    public VariableAccessExpression(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public VariableDeclaration Variable
    {
        get => _variable.Get();
        set => _variable.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}