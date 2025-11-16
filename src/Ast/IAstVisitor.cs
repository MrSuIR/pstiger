using PsTiger.Ast.Expressions;

namespace PsTiger.Ast;

public interface IAstVisitor
{
    void Visit(LiteralExpression e);

    void Visit(BinaryOperationExpression e);
}