using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PsTiger.Execution.Helpers;

/// <summary>
/// Позволяет использовать C# объекты как ключи в Dictionary, и в этой роли объекты-ключи сравниваются по ссылке,
///  а не по значению.
/// </summary>
/// <typeparam name="T">Класс объекта, используемого в роли ключа.</typeparam>
public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public bool Equals(T? x, T? y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}