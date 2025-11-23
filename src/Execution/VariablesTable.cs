using PsTiger.Runtime;

namespace PsTiger.Execution;

public sealed class VariablesTable
{
    private readonly VariablesTable? _parent;
    private readonly Dictionary<string, Value> _variables;

    public VariablesTable(VariablesTable? parent = null)
    {
        _parent = parent;
        _variables = [];
    }

    public VariablesTable? Parent => _parent;

    public Value GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out Value? value))
        {
            return value;
        }

        if (_parent != null)
        {
            return _parent.GetVariable(name);
        }

        throw new InvalidOperationException($"No variable with name {name}");
    }

    public void DefineVariable(string name, Value value)
    {
        if (!_variables.TryAdd(name, value))
        {
            throw new InvalidOperationException($"Variable with name {name} already defined");
        }
    }

    public void AssignVariable(string name, Value value)
    {
        if (_variables.ContainsKey(name))
        {
            _variables[name] = value;
        }
        else if (_parent != null)
        {
            _parent.AssignVariable(name, value);
        }
        else
        {
            throw new InvalidOperationException($"No variable with name {name}");
        }
    }
}