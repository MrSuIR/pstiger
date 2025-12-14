namespace PsTiger.Ast.Expressions;

public class FieldInitializer : AstNode
{
    public FieldInitializer(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; init; }

    public Expression Value { get; init; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}