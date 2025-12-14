using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Semantics.Helpers;

public static class ValueTypeUtil
{
    public static bool AreCompatibleTypes(ValueType a, ValueType b)
    {
        return (a == b)
               || (a is RecordType && b == ValueType.Nil)
               || (a == ValueType.Nil && b is RecordType);
    }
}