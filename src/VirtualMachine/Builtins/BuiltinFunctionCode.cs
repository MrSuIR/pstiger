namespace PsTiger.VirtualMachine.Builtins;

/// <summary>
/// Код встроенной функции.
/// </summary>
public enum BuiltinFunctionCode
{
    /// <summary>
    /// `print(s: string)` — выводит строку в стандартный поток вывода
    /// </summary>
    Print = 0,

    /// <summary>
    /// `printi(i: int)` — выводит целое число в стандартный поток вывода
    /// </summary>
    PrintI = 1,

    /// <summary>
    /// `flush()` — записывает данные в буфере стандартного потока вывода
    /// </summary>
    Flush = 2,

    /// <summary>
    /// `getchar(): string` — читает один символ из stdin
    /// </summary>
    GetChar = 3,

    /// <summary>
    /// `ord(s: string): int` — возвращает ASCII-код первого символа `s`
    /// </summary>
    Ord = 4,

    /// <summary>
    /// `chr(i: int): string` — возвращает строку из одного символа для ASCII-значения `i`
    /// </summary>
    Chr = 5,

    /// <summary>
    /// `size(s: string): int` — возвращает количество символов в строке `s`
    /// </summary>
    Size = 6,

    /// <summary>
    /// `substring(s: string, f: int, n: int): string` — возвращает подстроку `s`, начинающуюся с индекса `f`, длиной `n`
    /// </summary>
    Substring = 7,

    /// <summary>
    /// `concat(s1: string, s2: string): string` — возвращает результат конкатенации строк `s1` и `s2`
    /// </summary>
    Concat = 8,
}