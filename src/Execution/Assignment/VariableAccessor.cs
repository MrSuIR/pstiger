using PsTiger.Execution.Data;
using PsTiger.Runtime;

namespace PsTiger.Execution.Assignment;

/// <summary>
/// Читает переменную либо присваивает значение переменной.
/// </summary>
public class VariableAccessor : IValueAccessor
{
    private readonly VariablesTable _variables;
    private readonly string _name;

    public VariableAccessor(VariablesTable variables, string name)
    {
        _variables = variables;
        _name = name;
    }

    public Value Load()
    {
        return _variables.GetVariable(_name);
    }

    public void Store(Value value)
    {
        _variables.AssignVariable(_name, value);
    }
}