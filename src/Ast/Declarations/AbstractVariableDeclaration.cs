namespace PsTiger.Ast.Declarations;

/// <summary>
/// Абстрактный класс с информацией о переменной или формальном параметре функции.
/// </summary>
public abstract class AbstractVariableDeclaration : Declaration
{
    protected AbstractVariableDeclaration(string name)
    {
        this.Name = name;
    }

    public string Name { get; }
}