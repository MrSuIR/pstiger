using PsTiger.Runtime;

namespace PsTiger.Execution.Assignment;

/// <summary>
/// Читает элемент массива либо присваивает значение элементу массива.
/// </summary>
public class ArrayElementAccessor : IValueAccessor
{
    private readonly IValueAccessor _array;
    private readonly int _index;

    public ArrayElementAccessor(IValueAccessor array, int index)
    {
        _array = array;
        _index = index;
    }

    public Value Load()
    {
        Value array = _array.Load();
        return array.GetElement(_index);
    }

    public void Store(Value value)
    {
        Value array = _array.Load();
        array.SetElement(_index, value);
    }
}