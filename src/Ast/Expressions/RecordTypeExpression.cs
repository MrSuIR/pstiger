using PsTiger.Ast.Attributes;
using PsTiger.Ast.Declarations;

namespace PsTiger.Ast.Expressions;

public sealed class RecordTypeExpression : AbstractTypeExpression
{
    private AstAttribute<Dictionary<string, AbstractTypeDeclaration>> _fields;

    public RecordTypeExpression(List<FieldDeclaration> fieldDeclarations)
    {
        FieldDeclarations = fieldDeclarations;
    }

    public List<FieldDeclaration> FieldDeclarations { get; }

    /// <summary>
    /// Названия и типы полей структуры.
    /// </summary>
    public Dictionary<string, AbstractTypeDeclaration> Fields
    {
        get => _fields.Get();
        set => _fields.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}