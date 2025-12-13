using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public sealed class ArrayLiteralExpression : Expression
{
    private AstAttribute<AbstractTypeDeclaration> _arrayType;

    public ArrayLiteralExpression(string arrayTypeName, Expression size, Expression initialValue)
    {
        ArrayTypeName = arrayTypeName;
        Size = size;
        InitialValue = initialValue;
    }

    public string ArrayTypeName { get; set; }

    public Expression Size { get; set; }

    public Expression InitialValue { get; set; }

    public AbstractTypeDeclaration ArrayType
    {
        get => _arrayType.Get();
        set => _arrayType.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}