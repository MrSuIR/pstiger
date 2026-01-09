namespace PsTiger.Codegen;

/// <summary>
/// Таблица символов, основанная на лексических областях видимости (областях действия) символов в коде.
/// </summary>
public sealed class CodegenSymbolsTable
{
    private readonly CodegenSymbolsTable? _parent;
    private readonly int _depth;

    private readonly Dictionary<string, Function> _functions = [];

    public CodegenSymbolsTable(CodegenSymbolsTable? parent)
    {
        _parent = parent;
        _depth = (parent?.Depth ?? 0) + 1;
    }

    public int Depth => _depth;

    public CodegenSymbolsTable? Parent => _parent;

    public void DefineFunction(string name, BasicBlock block)
    {
        Function function = new(block, _depth);
        _functions[name] = function;
    }

    public Function GetFunction(string name)
    {
        if (_functions.TryGetValue(name, out Function? function))
        {
            return function;
        }

        if (_parent != null)
        {
            return _parent.GetFunction(name);
        }

        throw new InvalidOperationException($"No basic block for function {name}");
    }
}