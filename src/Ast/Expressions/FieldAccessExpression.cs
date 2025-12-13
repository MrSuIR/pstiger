namespace PsTiger.Ast.Expressions;

public sealed class FieldAccessExpression : Expression
{
    public FieldAccessExpression(Expression record, string fieldName)
    {
        Record = record;
        FieldName = fieldName;
    }

    public Expression Record { get; }

    public string FieldName { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}