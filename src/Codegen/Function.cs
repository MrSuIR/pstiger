namespace PsTiger.Codegen;

/// <summary>
/// Функция представлена ссылкой на базовый блок начала функции и глубиной лексической области видимости.
/// </summary>
/// <remarks>
/// В отличие от LLVM, в нашем бэкенде имя функции не важно.
/// С другой стороны, виртуальная машина реализует захват переменных из окружающей функцию области видимости,
///  поэтому для каждой функции мы сохраняем глубину окружающей лексической области видимости.
/// </remarks>
public class Function
{
    public Function(BasicBlock entry, int parentScopeDepth)
    {
        Entry = entry;
        ParentScopeDepth = parentScopeDepth;
    }

    public BasicBlock Entry { get; }

    public int ParentScopeDepth { get; }
}