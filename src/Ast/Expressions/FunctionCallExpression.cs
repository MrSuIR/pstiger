namespace PsTiger.Ast.Expressions;

/// <summary>
/// Выражение вызова функции со списком аргументов.
/// </summary>
public class FunctionCallExpression : Expression
{
    public FunctionCallExpression(string name, IReadOnlyList<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public IReadOnlyList<Expression> Arguments { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}