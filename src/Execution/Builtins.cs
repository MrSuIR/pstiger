using PsTiger.Ast.Declarations;
using PsTiger.Runtime;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Execution;

/// <summary>
/// Объект, предоставляющий доступ к встроенным символам языка.
/// </summary>
public class Builtins
{
    public Builtins(IEnvironment environment)
    {
        List<BuiltinFunction> functions =
        [
            new(
                "print", // print(s: string)` — выводит строку в стандартный поток вывода
                [
                    new ParameterDeclaration("s", ValueType.String),
                ],
                ValueType.Void,
                arguments =>
                {
                    environment.Print(arguments[0].AsString());
                    return new Value();
                }
            ),

            new(
                "printi", // `printi(i: int)` — выводит целое число в стандартный поток вывода
                [
                    new ParameterDeclaration("i", ValueType.Int),
                ],
                ValueType.Void,
                arguments =>
                {
                    environment.PrintInt(arguments[0].AsInt());
                    return new Value();
                }
            ),

            new(
                "flush", // `flush()` — записывает данные в буфере стандартного потока вывода
                [],
                ValueType.Void,
                _ =>
                {
                    environment.Flush();
                    return new Value();
                }
            ),

            new(
                "not", // `not(i: int): int` — если `i = 0`, то возвращает `1`, иначе возвращает `0`
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
    public IReadOnlyDictionary<string, BuiltinFunction> Functions { get; }
}