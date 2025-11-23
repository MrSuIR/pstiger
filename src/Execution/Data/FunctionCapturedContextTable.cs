using PsTiger.Ast.Declarations;
using PsTiger.Execution.Helpers;

namespace PsTiger.Execution.Data;

/// <summary>
/// Отображает объявление функции на захваченный ей контекст переменных (это называется замыканием, или closure).
/// </summary>
public class FunctionCapturedContextTable
{
    /// <summary>
    /// Ссылка на родительскую таблицу.
    /// </summary>
    private readonly FunctionCapturedContextTable? _parent;

    /// <summary>
    /// Отображает AST-узел объявления функции на таблицу переменных данной функции.
    /// </summary>
    private readonly Dictionary<AbstractFunctionDeclaration, VariablesTable> _capturedVariablesTableMap;

    public FunctionCapturedContextTable(FunctionCapturedContextTable? parent = null)
    {
        _parent = parent;
        _capturedVariablesTableMap = new Dictionary<AbstractFunctionDeclaration, VariablesTable>(
            new ReferenceEqualityComparer<AbstractFunctionDeclaration>()
        );
    }

    public FunctionCapturedContextTable? Parent => _parent;

    public void CaptureVariablesTable(AbstractFunctionDeclaration function, VariablesTable variables)
    {
        _capturedVariablesTableMap.Add(function, variables);
    }

    public VariablesTable GetCapturedVariablesTable(AbstractFunctionDeclaration function)
    {
        if (_capturedVariablesTableMap.TryGetValue(function, out VariablesTable? table))
        {
            return table;
        }

        if (_parent != null)
        {
            return _parent.GetCapturedVariablesTable(function);
        }

        throw new Exception($"Function with name {function.Name} has no captured context");
    }
}