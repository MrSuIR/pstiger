using System.Reflection.Emit;

namespace PsTiger.MsilCodegen;

public class LocalVariablesScope
{
    // Словарь локальных переменных данной области видимости.
    private readonly Dictionary<string, LocalBuilder> _variables = [];

    public LocalVariablesScope(LocalVariablesScope? parent = null)
    {
        Parent = parent;
    }

    public LocalVariablesScope? Parent { get; }

    /// <summary>
    /// Получает объявление переменной в MSIL по её имени.
    /// </summary>
    public LocalBuilder GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out LocalBuilder? variable))
        {
            return variable;
        }

        if (Parent != null)
        {
            return Parent.GetVariable(name);
        }

        throw new InvalidOperationException($"No variable with name {name} defined");
    }

    /// <summary>
    /// Добавляет объявление переменной в MSIL с указанным именем в стек.
    /// </summary>
    public void AddVariable(string name, LocalBuilder variable)
    {
        if (!_variables.TryAdd(name, variable))
        {
            throw new InvalidOperationException($"Variable with name {name} already defined");
        }
    }
}