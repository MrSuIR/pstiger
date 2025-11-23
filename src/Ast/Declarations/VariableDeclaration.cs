using PsTiger.Ast.Expressions;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
///  Узел дерева, представляющий объявление переменной.
///  У переменной может быть указан тип и всегда указано начальное значение.
/// </summary>
public sealed class VariableDeclaration : Declaration
{
    public VariableDeclaration(string name, ValueType? declaredType, Expression initialValue)
    {
        Name = name;
        DeclaredType = declaredType;
        InitialValue = initialValue;
    }

    public string Name { get; }

    public ValueType? DeclaredType { get; }

    public Expression InitialValue { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}