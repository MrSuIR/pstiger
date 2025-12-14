using PsTiger.Runtime;

namespace PsTiger.Execution.Assignment;

/// <summary>
/// Читает поле структуры либо присваивает ему значение.
/// </summary>
public class RecordFieldAccessor : IValueAccessor
{
    private readonly IValueAccessor _record;
    private readonly string _fieldName;

    public RecordFieldAccessor(IValueAccessor record, string fieldName)
    {
        _record = record;
        _fieldName = fieldName;
    }

    public Value Load()
    {
        Value record = _record.Load();
        return record.GetField(_fieldName);
    }

    public void Store(Value value)
    {
        Value record = _record.Load();
        record.SetField(_fieldName, value);
    }
}