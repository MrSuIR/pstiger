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
                Builtins.Ord, _ => throw new NotImplementedException("Cannot emit MSIL for \"ord\" function")
            },
            {
                Builtins.Chr, _ => throw new NotImplementedException("Cannot emit MSIL for \"chr\" function")
            },
            {
                Builtins.Concat, _ => throw new NotImplementedException("Cannot emit MSIL for \"concat)\" function")
            },
            {
                Builtins.Size, _ => throw new NotImplementedException("Cannot emit MSIL for \"size\" function")
            },
            {
                Builtins.Substring, _ => throw new NotImplementedException(
                    "Cannot emit MSIL for \"substring\" function"
                )
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
        MethodInfo writeMethod = GetMethod(typeof(Console), "Write", [typeof(string)]);
        il.Emit(OpCodes.Call, writeMethod);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции printi(i : int).
    /// </summary>
    private void EmitPrintI(ILGenerator il)
    {
        // Находим метод Console.Write(int) и вызываем его.
        MethodInfo writeMethod = GetMethod(typeof(Console), "Write", [typeof(int)]);
        il.Emit(OpCodes.Call, writeMethod);
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
        throw new NotImplementedException("Cannot generate \"getchar(): string\" function call yet");
    }

    /// <summary>
    /// Генерирует вызов встроенной функции exit(code : int).
    /// </summary>
    private void EmitExit(ILGenerator il)
    {
        // Находим метод Environment.Exit(int) и вызываем его.
        MethodInfo exitMethod = GetMethod(typeof(Environment), "Exit", [typeof(int)]);
        il.Emit(OpCodes.Call, exitMethod);
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
            throw new InvalidOperationException($"Cannot find method {type.Name}.{methodName}({parameterTypeNames}.");
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