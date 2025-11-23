using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;

namespace PsTiger.Ast;

public interface IAstVisitor
{
    void Visit(LiteralExpression e);

    void Visit(BinaryOperationExpression e);

    void Visit(SequenceExpression e);

    void Visit(UnaryMinusExpression e);

    void Visit(FunctionCallExpression e);

    void Visit(ScopeExpression e);

    void Visit(VariableDeclaration e);
}