namespace PsTiger.Ast.Declarations;

public sealed class ForIteratorDeclaration : AbstractVariableDeclaration
{
    public ForIteratorDeclaration(string name)
        : base(name)
    {
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}