using PsTiger.Ast.Declarations;

using ValueType = PsTiger.Runtime.ValueType;

namespace PsTiger.Ast;

/// <summary>
/// Объект, предоставляющий доступ к встроенным символам языка.
/// </summary>
public static class Builtins
{
    public const string Print = "print";
    public const string PrintI = "printi";
    public const string Flush = "flush";
    public const string GetChar = "getchar";
    public const string Ord = "ord";
    public const string Chr = "chr";
    public const string Size = "size";
    public const string Substring = "substring";
    public const string Concat = "concat";
    public const string Not = "not";
    public const string Exit = "exit";

    /// <summary>
    /// Список встроенных функций языка.
    /// </summary>
    public static readonly IReadOnlyList<BuiltinFunction> Functions =
    [
        new(
            Print, // `print(s: string)` — выводит строку в стандартный поток вывода
            [
                new BuiltinFunctionParameter("s", ValueType.String),
            ],
            ValueType.Void
        ),

        new(
            PrintI, // `printi(i: int)` — выводит целое число в стандартный поток вывода
            [
                new BuiltinFunctionParameter("i", ValueType.Int),
            ],
            ValueType.Void
        ),

        new(
            Flush, // `flush()` — записывает данные в буфере стандартного потока вывода
            [],
            ValueType.Void
        ),

        new(
            GetChar, // `getchar(): string` — читает один символ из stdin
            [],
            ValueType.String
        ),

        new(
            Ord, // `ord(s: string): int` — возвращает ASCII-код первого символа `s`
            [
                new BuiltinFunctionParameter("s", ValueType.String),
            ],
            ValueType.Int
        ),

        new(
            Chr, // `chr(i: int): string` — возвращает строку из одного символа для ASCII-значения `i`
            [
                new BuiltinFunctionParameter("i", ValueType.Int),
            ],
            ValueType.String
        ),

        new(
            Size, // `size(s: string): int` — возвращает количество символов в строке `s`
            [
                new BuiltinFunctionParameter("s", ValueType.String),
            ],
            ValueType.Int
        ),

        new(
            Substring, // `substring(s: string, f: int, n: int): string` — возвращает подстроку `s`, начинающуюся с индекса `f`, длиной `n`
            [
                new BuiltinFunctionParameter("s", ValueType.String),
                new BuiltinFunctionParameter("f", ValueType.Int),
                new BuiltinFunctionParameter("n", ValueType.Int),
            ],
            ValueType.String
        ),

        new(
            Concat, // `concat(s1: string, s2: string): string` — возвращает результат конкатенации строк `s1` и `s2`
            [
                new BuiltinFunctionParameter("s1", ValueType.String),
                new BuiltinFunctionParameter("s2", ValueType.String),
            ],
            ValueType.String
        ),

        new(
            Not, // `not(i: int): int` — если `i = 0`, то возвращает `1`, иначе возвращает `0`
            [
                new BuiltinFunctionParameter("i", ValueType.Int),
            ],
            ValueType.Int
        ),

        new(
            Exit, // `exit(i: int)` — завершает программу с кодом выхода `i`
            [
                new BuiltinFunctionParameter("i", ValueType.Int),
            ],
            ValueType.Void
        ),
    ];

    /// <summary>
    /// Список встроенных типов языка.
    /// </summary>
    public static readonly IReadOnlyList<BuiltinType> Types =
    [
        new("int", ValueType.Int),
        new("string", ValueType.String),
    ];
}