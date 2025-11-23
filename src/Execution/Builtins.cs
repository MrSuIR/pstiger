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
                    new BuiltinFunctionParameter("s", ValueType.String),
                ],
                ValueType.Void,
                arguments =>
                {
                    environment.Print(arguments[0].AsString());
                    return Value.Void;
                }
            ),

            new(
                "printi", // `printi(i: int)` — выводит целое число в стандартный поток вывода
                [
                    new BuiltinFunctionParameter("i", ValueType.Int),
                ],
                ValueType.Void,
                arguments =>
                {
                    environment.PrintInt(arguments[0].AsInt());
                    return Value.Void;
                }
            ),

            new(
                "flush", // `flush()` — записывает данные в буфере стандартного потока вывода
                [],
                ValueType.Void,
                _ =>
                {
                    environment.Flush();
                    return Value.Void;
                }
            ),

            new(
                "getchar", // `getchar(): string` — читает один символ из stdin
                [],
                ValueType.String,
                _ =>
                {
                    int code = environment.ReadChar();
                    if (code == -1)
                    {
                        return new Value("");
                    }

                    char c = (code < 128) ? (char)code : '?';
                    return new Value(c.ToString());
                }
            ),

            new(
                "ord", // `ord(s: string): int` — возвращает ASCII-код первого символа `s`
                [
                    new BuiltinFunctionParameter("s", ValueType.String),
                ],
                ValueType.Int,
                arguments =>
                {
                    string text = arguments[0].AsString();
                    int result = (text.Length > 0) ? text[0] : -1;
                    return new Value(result);
                }
            ),

            new(
                "chr", // `chr(i: int): string` — возвращает строку из одного символа для ASCII-значения `i`
                [
                    new BuiltinFunctionParameter("i", ValueType.Int),
                ],
                ValueType.String,
                arguments =>
                {
                    int code = arguments[0].AsInt();
                    if (code < 0 || code >= 128)
                    {
                        throw new ProgramAbortedException($"Invalid character code {code}");
                    }

                    char ch = (char)code;
                    return new Value(ch.ToString());
                }
            ),

            new(
                "size", // `size(s: string): int` — возвращает количество символов в строке `s`
                [
                    new BuiltinFunctionParameter("s", ValueType.String),
                ],
                ValueType.Int,
                arguments =>
                {
                    string text = arguments[0].AsString();
                    return new Value(text.Length);
                }
            ),

            new(
                "substring", // `substring(s: string, f: int, n: int): string` — возвращает подстроку `s`, начинающуюся с индекса `f`, длиной `n`
                [
                    new BuiltinFunctionParameter("s", ValueType.String),
                    new BuiltinFunctionParameter("f", ValueType.Int),
                    new BuiltinFunctionParameter("n", ValueType.Int),
                ],
                ValueType.String,
                arguments =>
                {
                    string text = arguments[0].AsString();
                    int fromIndex = arguments[1].AsInt();
                    int length = arguments[2].AsInt();

                    // Разрешаем выход за границы строки, в этом случае результат будет короче заданной длины.
                    int safeLength = int.Min(length, text.Length - fromIndex);

                    return new Value(text.Substring(fromIndex, safeLength));
                }
            ),

            new(
                "concat", // `concat(s1: string, s2: string): string` — возвращает результат конкатенации строк `s1` и `s2`
                [
                    new BuiltinFunctionParameter("s1", ValueType.String),
                    new BuiltinFunctionParameter("s2", ValueType.String),
                ],
                ValueType.String,
                arguments =>
                {
                    string left = arguments[0].AsString();
                    string right = arguments[1].AsString();
                    return new Value(left + right);
                }
            ),

            new(
                "not", // `not(i: int): int` — если `i = 0`, то возвращает `1`, иначе возвращает `0`
                [
                    new BuiltinFunctionParameter("i", ValueType.Int),
                ],
                ValueType.Int,
                arguments => (arguments[0].AsInt() == 0) ? new Value(1) : new Value(0)
            ),

            new(
                "exit", // `exit(i: int)` — завершает программу с кодом выхода `i`
                [
                    new BuiltinFunctionParameter("i", ValueType.Int),
                ],
                ValueType.Void,
                arguments =>
                {
                    int exitCode = arguments[0].AsInt();
                    throw new ProgramExitedException(exitCode);
                }
            ),
        ];

        Functions = functions.ToDictionary(function => function.Name);

        List<BuiltinType> types =
        [
            new("int", ValueType.Int),
            new("string", ValueType.String),
        ];

        Types = types.ToDictionary(type => type.Name);
    }

    /// <summary>
    /// Список встроенных функций языка.
    /// </summary>
    public IReadOnlyDictionary<string, BuiltinFunction> Functions { get; }

    /// <summary>
    /// Список встроенных типов языка.
    /// </summary>
    public IReadOnlyDictionary<string, BuiltinType> Types { get; }
}