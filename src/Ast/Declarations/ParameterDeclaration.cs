using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast.Declarations;

/// <summary>
/// Объявление параметра функции.
/// </summary>
public class ParameterDeclaration
{
    public ParameterDeclaration(string name, ValueType valueType)
    {
        this.Name = name;
        this.ValueType = valueType;
    }

    public string Name { get; }

    public ValueType ValueType { get; }
}