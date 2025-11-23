using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

/// <summary>
/// Выражение вызова функции со списком аргументов.
/// </summary>
public class FunctionCallExpression : Expression
{
    private AstAttribute<AbstractFunctionDeclaration> _function;

    public FunctionCallExpression(string name, IReadOnlyList<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public AbstractFunctionDeclaration Function
    {
        get => _function.Get();
        set => _function.Set(value);
    }

    public string Name { get; }

    public IReadOnlyList<Expression> Arguments { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}