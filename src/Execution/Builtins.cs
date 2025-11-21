using PsTiger.Ast.Declarations;
using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Execution;

/// <summary>
/// Статический клас, предоставляющий список встроенных символов языка.
/// </summary>
public static class Builtins
{
    /// <summary>
    /// Статический конструктор, инициализирующий статические поля класса.
    /// </summary>
    static Builtins()
    {
        List<BuiltinFunction> functions =
        [
            new(
                "not",
                [
                    new ParameterDeclaration("i", ValueType.Int),
                ],
                ValueType.Int,
                arguments => (arguments[0].AsInt() == 0) ? new Value(1) : new Value(0)
            ),
        ];

        Functions = functions.ToDictionary(function => function.Name);
    }

    /// <summary>
    /// Список встроенных функций языка.
    /// </summary>
    public static IReadOnlyDictionary<string, BuiltinFunction> Functions { get; }
}