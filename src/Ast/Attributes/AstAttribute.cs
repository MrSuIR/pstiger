using System.Runtime.CompilerServices;

namespace PsTiger.Ast.Attributes;

/// <summary>
/// Каждый атрибут AST устанавливается один раз на фазе семантического анализа,
///  после чего он становится доступным для чтения.
/// </summary>
/// <typeparam name="T">Тип значения атрибута</typeparam>
public struct AstAttribute<T>
{
    private T _value;
    private bool _initialized;

    public T Get([CallerMemberName] string? memberName = null)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"Attribute {memberName} with type {typeof(T)} value is not set");
        }

        return _value;
    }

    public void Set(T value, [CallerMemberName] string? memberName = null)
    {
        if (_initialized)
        {
            throw new InvalidOperationException($"Attribute {memberName} with type {typeof(T)} already has a value");
        }

        _value = value;
        _initialized = true;
    }
}