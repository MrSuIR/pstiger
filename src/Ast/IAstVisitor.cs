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

    void Visit(VariableAccessExpression e);

    void Visit(AssignmentExpression e);

    void Visit(IfElseExpression e);

    void Visit(VariableDeclaration d);

    void Visit(FunctionDeclaration d);

    void Visit(ParameterDeclaration d);

    void Visit(WhileLoopExpression e);

    void Visit(ForLoopExpression e);

    void Visit(ForIteratorDeclaration d);
}