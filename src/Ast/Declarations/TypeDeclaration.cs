using PsTiger.Ast.Expressions;

namespace PsTiger.Ast.Declarations;

public sealed class TypeDeclaration : AbstractTypeDeclaration
{
    public TypeDeclaration(string name, AbstractTypeExpression typeExpression)
        : base(name)
    {
        TypeExpression = typeExpression;
    }

    public AbstractTypeExpression TypeExpression { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}