namespace PsTiger.Ast.Expressions;

public class FieldInitializer
{
    public FieldInitializer(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; init; }

    public Expression Value { get; init; }
}