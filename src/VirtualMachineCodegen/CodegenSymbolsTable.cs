namespace PsTiger.VirtualMachineCodegen;

/// <summary>
/// Таблица символов, основанная на лексических областях видимости (областях действия) символов в коде.
/// </summary>
public sealed class CodegenSymbolsTable
{
    private readonly CodegenSymbolsTable? _parent;
    private readonly int _depth;

    private readonly Dictionary<string, BasicBlock> _functions = [];

    public CodegenSymbolsTable(CodegenSymbolsTable? parent)
    {
        _parent = parent;
        _depth = (parent?.Depth ?? 0) + 1;
    }

    public int Depth => _depth;

    public CodegenSymbolsTable? Parent => _parent;

    /// <summary>
    /// Добавляет ссылку на базовый блок, с которого начинается указанная функция.
    /// </summary>
    public void AddFunctionEntry(string name, BasicBlock block)
    {
        _functions.Add(name, block);
    }

    /// <summary>
    /// Получает ссылку на базовый блок, с которого начинается указанная функция.
    /// </summary>
    public BasicBlock GetFunctionEntry(string name)
    {
        if (_functions.TryGetValue(name, out BasicBlock? block))
        {
            return block;
        }

        if (_parent != null)
        {
            return _parent.GetFunctionEntry(name);
        }

        throw new InvalidOperationException($"No basic block for function {name}");
    }
}