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

    void Visit(BreakLoopExpression e);

    void Visit(TypeDeclaration d);

    void Visit(NamedTypeExpression e);

    void Visit(ArrayTypeExpression e);

    void Visit(ArrayAccessExpression e);

    void Visit(ArrayLiteralExpression e);

    void Visit(RecordTypeExpression e);

    void Visit(FieldDeclaration d);

    void Visit(RecordLiteralExpression e);

    void Visit(FieldAccessExpression e);
}