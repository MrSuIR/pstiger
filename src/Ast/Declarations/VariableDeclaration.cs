using PsTiger.Ast.Attributes;
using PsTiger.Ast.Expressions;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
///  Узел дерева, представляющий объявление переменной.
///  У переменной может быть указан тип и всегда указано начальное значение.
/// </summary>
public sealed class VariableDeclaration : AbstractVariableDeclaration
{
    private AstAttribute<AbstractTypeDeclaration?> _declaredType;

    public VariableDeclaration(string name, string? declaredTypeName, Expression initialValue)
        : base(name)
    {
        DeclaredTypeName = declaredTypeName;
        InitialValue = initialValue;
    }

    public string? DeclaredTypeName { get; }

    public Expression InitialValue { get; }

    public AbstractTypeDeclaration? DeclaredType
    {
        get => _declaredType.Get();
        set => _declaredType.Set(value);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}