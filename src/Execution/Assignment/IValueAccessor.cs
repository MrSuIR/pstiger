using PsTiger.Runtime;

namespace PsTiger.Execution.Assignment;

/// <summary>
/// Предоставляет доступ к чтению/записи значения, хранимого в определённом месте.
/// </summary>
public interface IValueAccessor
{
    public Value Load();

    public void Store(Value value);
}