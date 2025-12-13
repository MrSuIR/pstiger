using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public sealed class RecordLiteralExpression : Expression
{
    private AstAttribute<AbstractTypeDeclaration> _recordType;

    public RecordLiteralExpression(string recordTypeName, List<FieldInitializer> initializers)
    {
        RecordTypeName = recordTypeName;
        Initializers = initializers;
    }

    public string RecordTypeName { get; }

    public List<FieldInitializer> Initializers { get; }

    public AbstractTypeDeclaration RecordType
    {
        get => _recordType.Get();
        set => _recordType.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}