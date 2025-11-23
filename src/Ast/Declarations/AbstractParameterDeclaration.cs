namespace PsTiger.Ast.Declarations;

/// <summary>
/// Абстрактный класс с информацией о формальном параметре функции — как встроенной, так и пользовательской.
/// </summary>
public abstract class AbstractParameterDeclaration : Declaration
{
    protected AbstractParameterDeclaration(string name)
    {
        this.Name = name;
    }

    public string Name { get; }
}