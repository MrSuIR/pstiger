namespace PsTiger.Ast.Declarations;

/// <summary>
/// Абстрактный класс с информацией о формальном параметре функции — как встроенной, так и пользовательской.
/// </summary>
public abstract class AbstractParameterDeclaration : AbstractVariableDeclaration
{
    protected AbstractParameterDeclaration(string name)
        : base(name)
    {
    }
}