using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;

using PsTiger.Ast;

namespace PsTiger.MsilCodegen;

public class BuiltinFunctionEmitter
{
    private readonly Dictionary<string, Action<ILGenerator>> _functionsMap;

    public BuiltinFunctionEmitter()
    {
        _functionsMap = new Dictionary<string, Action<ILGenerator>>
        {
            {
                Builtins.Print, EmitPrint
            },
            {
                Builtins.PrintI, EmitPrintI
            },
            {
                Builtins.Flush, EmitFlush
            },
            {
                Builtins.GetChar, EmitGetChar
            },
            {
                Builtins.Exit, EmitExit
            },
            {
                Builtins.Not, _ => throw new NotImplementedException("Cannot emit MSIL for \"not\" function")
            },
            {
                Builtins.Ord, EmitOrd
            },
            {
                Builtins.Chr, EmitChr
            },
            {
                Builtins.Concat, EmitConcat
            },
            {
                Builtins.Size, EmitSize
            },
            {
                Builtins.Substring, EmitSubstring
            },
        };
    }

    public bool IsBuiltinFunction(string name)
    {
        return _functionsMap.ContainsKey(name);
    }

    public void EmitCallBuiltinFunction(string name, ILGenerator il)
    {
        Action<ILGenerator> action = _functionsMap[name];
        action(il);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции print(s : string).
    /// </summary>
    private void EmitPrint(ILGenerator il)
    {
        // Находим метод Console.Write(string) и вызываем его.
        MethodInfo method = GetMethod(typeof(Console), "Write", [typeof(string)]);
        il.Emit(OpCodes.Call, method);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции printi(i : int).
    /// </summary>
    private void EmitPrintI(ILGenerator il)
    {
        // Находим метод Console.Write(int) и вызываем его.
        MethodInfo method = GetMethod(typeof(Console), "Write", [typeof(int)]);
        il.Emit(OpCodes.Call, method);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции flush().
    /// </summary>
    private void EmitFlush(ILGenerator il)
    {
        // Добавляем в стек ссылку на объект `Console.Out` и вызываем для него виртуальный метод TextWriter.Flush().
        MethodInfo consoleOutGetter = GetPropertyGetterMethod(typeof(Console), "Out");
        MethodInfo flushMethod = GetMethod(typeof(TextWriter), "Flush", Type.EmptyTypes);

        il.Emit(OpCodes.Call, consoleOutGetter);
        il.Emit(OpCodes.Callvirt, flushMethod);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции getchar() : string.
    /// </summary>
    private void EmitGetChar(ILGenerator il)
    {
        // Получаем метод `int Console.Read()` и вызываем его.
        MethodInfo read = GetMethod(typeof(Console), "Read", Type.EmptyTypes);
        il.Emit(OpCodes.Call, read);

        Label eofLabel = il.DefineLabel();
        Label invalidCharLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();

        // Если код символа равен "-1", значит, достигнут конец файла — тогда переходим к блоку EOF.
        il.Emit(OpCodes.Dup); // Дублируем значение на стеке.
        il.Emit(OpCodes.Ldc_I4_M1); // Добавляем в стек -1
        il.Emit(OpCodes.Ceq); // Сравниваем значения на равенство.
        il.Emit(OpCodes.Brtrue, eofLabel);

        // Если код символа больше 127, то возвращаем "?"
        il.Emit(OpCodes.Dup); // Дублируем значение на стеке.
        il.Emit(OpCodes.Ldc_I4, 128); // Добавляем в стек 128.
        il.Emit(OpCodes.Clt); // Сравниваем значения (меньше чем).
        il.Emit(OpCodes.Brfalse, invalidCharLabel);

        // Получаем метод `string char.ConvertFromUtf32(int)` и вызываем его,
        //  чтобы конвертировать код символа в строку.
        MethodInfo convertFromUtf32 = GetMethod(typeof(char), "ConvertFromUtf32", [typeof(int)]);
        il.Emit(OpCodes.Call, convertFromUtf32);
        il.Emit(OpCodes.Br, endLabel);

        // Блок invalid char: возвращаем "?".
        il.MarkLabel(invalidCharLabel);
        il.Emit(OpCodes.Pop); // Убираем код символа из стека.
        il.Emit(OpCodes.Ldstr, "?");
        il.Emit(OpCodes.Br, endLabel);

        // Блок EOF: возвращаем пустую строку.
        il.MarkLabel(eofLabel);
        il.Emit(OpCodes.Pop); // Убираем код символа из стека.
        il.Emit(OpCodes.Ldstr, "");

        // Блок, завершающий текущую последовательность инструкций.
        il.MarkLabel(endLabel);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции exit(code : int).
    /// </summary>
    private void EmitExit(ILGenerator il)
    {
        // Находим метод Environment.Exit(int) и вызываем его.
        MethodInfo method = GetMethod(typeof(Environment), "Exit", [typeof(int)]);
        il.Emit(OpCodes.Call, method);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции ord(s : string) : int.
    /// </summary>
    private void EmitOrd(ILGenerator il)
    {
        Label emptyStringLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();

        // Вычисляем длину строки, чтобы проверить, является ли она пустой.
        il.Emit(OpCodes.Dup);
        EmitSize(il);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Brtrue, emptyStringLabel);

        // Строка не пустая. Получаем первый символ с помощью индексатора: text[0].
        // Больше ничего не делаем, поскольку по правилам MSIL тип char будет неявно преобразован к int
        //  при его последующем использовании.
        MethodInfo indexerGetter = GetMethod(typeof(string), "get_Chars", [typeof(int)]);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Callvirt, indexerGetter);
        il.Emit(OpCodes.Br, endLabel);

        // Блок empty string: если строка пустая, возвращаем -1.
        il.MarkLabel(emptyStringLabel);
        il.Emit(OpCodes.Pop); // Убираем исходную строку со стека.
        il.Emit(OpCodes.Ldc_I4_M1);

        // Блок, завершающий текущую последовательность инструкций.
        il.MarkLabel(endLabel);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции chr(i : int) : string.
    /// </summary>
    private void EmitChr(ILGenerator il)
    {
        Label terminateLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();

        // Если код символа больше 127, то аварийно завершаем программу
        il.Emit(OpCodes.Dup); // Дублируем значение на стеке.
        il.Emit(OpCodes.Ldc_I4, 128); // Добавляем в стек 128.
        il.Emit(OpCodes.Clt); // Сравниваем значения (меньше чем).
        il.Emit(OpCodes.Brfalse, terminateLabel);

        // Если код символа меньше 0, то аварийно завершаем программу
        il.Emit(OpCodes.Dup); // Дублируем значение на стеке.
        il.Emit(OpCodes.Ldc_I4, 0); // Добавляем в стек 0.
        il.Emit(OpCodes.Clt); // Сравниваем значения (меньше чем).
        il.Emit(OpCodes.Brtrue, terminateLabel);

        // Получаем метод `string char.ConvertFromUtf32(int)` и вызываем его,
        //  чтобы конвертировать код символа в строку.
        MethodInfo convertFromUtf32 = GetMethod(typeof(char), "ConvertFromUtf32", [typeof(int)]);
        il.Emit(OpCodes.Call, convertFromUtf32);
        il.Emit(OpCodes.Br, endLabel);

        // Блок terminate: аварийное завершение программы
        //  с сообщением "fatal error: invalid character code {code}" и кодом 1.
        il.MarkLabel(terminateLabel);
        il.Emit(OpCodes.Ldstr, "fatal error: invalid character code ");
        EmitPrint(il); // Печатаем начало сообщения об ошибке.
        EmitPrintI(il); // Печатаем код символа.
        il.Emit(OpCodes.Ldstr, ""); // Добавляем пустую строку как результат, чтобы стек .NET оставался валидным.
        il.Emit(OpCodes.Ldc_I4_1);
        EmitExit(il);

        // Блок, завершающий текущую последовательность инструкций.
        il.MarkLabel(endLabel);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции concat(s1 : string, s2 : string) : string.
    /// </summary>
    private void EmitConcat(ILGenerator il)
    {
        // Находим метод string.Concat(int) и вызываем его.
        MethodInfo method = GetMethod(typeof(string), "Concat", [typeof(string), typeof(string)]);
        il.Emit(OpCodes.Call, method);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции substring(s : string, first : int, n : int) : string.
    /// </summary>
    /// <remarks>
    /// Предполагается, что на стеке находятся (сверху вниз): n, first, s.
    /// Генерирует вызов string.Substring(first, n).
    /// </remarks>
    private void EmitSubstring(ILGenerator il)
    {
        MethodInfo method = GetMethod(typeof(string), "Substring", [typeof(int), typeof(int)]);
        il.Emit(OpCodes.Call, method);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции size(s : string) : int.
    /// </summary>
    private void EmitSize(ILGenerator il)
    {
        MethodInfo getter = GetPropertyGetterMethod(typeof(string), "Length");
        il.Emit(OpCodes.Call, getter);
    }

    /// <summary>
    /// Находит статический метод указанного типа стандартной библиотеки классов .NET,
    ///  чтобы использовать его для реализации встроенной функции языка Tiger.
    /// </summary>
    private static MethodInfo GetMethod(Type type, string methodName, Type[] parameterTypes)
    {
        MethodInfo? method = type.GetMethod(methodName, parameterTypes);
        if (method == null)
        {
            string parameterTypeNames = string.Join(", ", parameterTypes.Select(t => t.Name));
            throw new InvalidOperationException($"Cannot find method {type.Name}.{methodName}({parameterTypeNames}).");
        }

        return method;
    }

    private static MethodInfo GetPropertyGetterMethod(Type type, string propertyName)
    {
        PropertyInfo? outProperty = type.GetProperty(propertyName);
        if (outProperty == null)
        {
            throw new InvalidOperationException($"Cannot find property {type.Name}.{propertyName}.");
        }

        MethodInfo? getterMethod = outProperty.GetGetMethod();
        if (getterMethod == null)
        {
            throw new InvalidOperationException($"Property {type.Name}.{propertyName} has no getter.");
        }

        return getterMethod;
    }
}