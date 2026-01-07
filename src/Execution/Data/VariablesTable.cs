using PsTiger.Runtime;

namespace PsTiger.Execution.Data;

public sealed class VariablesTable
{
    private readonly VariablesTable? _parent;
    private readonly Dictionary<string, Value> _variables;
    private readonly int _depth;

    public VariablesTable(VariablesTable? parent = null)
    {
        _parent = parent;
        _depth = (parent?._depth ?? 0) + 1;
        _variables = [];
    }

    public VariablesTable? Parent => _parent;

    /// <summary>
    /// Получает таблицу с указанной глубиной.
    /// Это необходимо для поддержки захвата функцией окружающей её области видимости.
    /// </summary>
    public VariablesTable GetAncestor(int depth)
    {
        if (depth <= 0)
        {
            throw new InvalidOperationException($"Invalid variables table depth {depth}");
        }

        if (depth > _depth)
        {
            throw new InvalidOperationException($"No variables table with depth {depth}: current depth is {_depth}");
        }

        return GetAncestorImpl(depth);
    }

    /// <summary>
    /// Получает значение переменной по имени.
    /// </summary>
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

    /// <summary>
    /// Объявляет переменную с указанным именем и начальным значением.
    /// </summary>
    public void DefineVariable(string name, Value value)
    {
        if (!_variables.TryAdd(name, value))
        {
            throw new InvalidOperationException($"Variable with name {name} already defined");
        }
    }

    /// <summary>
    /// Присваивает значение переменной с указанным именем.
    /// </summary>
    public void AssignVariable(string name, Value value)
    {
        if (!TryAssignVariable(name, value))
        {
            throw new InvalidOperationException($"No variable with name {name}");
        }
    }

    private bool TryAssignVariable(string name, Value value)
    {
        if (_variables.ContainsKey(name))
        {
            _variables[name] = value;
            return true;
        }

        if (_parent != null)
        {
            return _parent.TryAssignVariable(name, value);
        }

        return false;
    }

    private VariablesTable GetAncestorImpl(int depth)
    {
        if (_depth == depth)
        {
            return this;
        }

        return _parent!.GetAncestor(depth);
    }
}