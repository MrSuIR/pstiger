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
                Builtins.GetChar, EmitGetChar
            },
            {
                Builtins.Exit, Exit
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
        MethodInfo writeMethod = GetStaticMethod(typeof(Console), "Write", [typeof(string)]);
        il.Emit(OpCodes.Call, writeMethod);
    }

    /// <summary>
    /// Генерирует вызов встроенной функции printi(i : int).
    /// </summary>
    private void EmitPrintI(ILGenerator il)
    {
        // Находим метод Console.Write(int) и вызываем его.
        MethodInfo writeMethod = GetStaticMethod(typeof(Console), "Write", [typeof(int)]);
        il.Emit(OpCodes.Call, writeMethod);
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
    private void Exit(ILGenerator il)
    {
        // Находим метод Environment.Exit(int) и вызываем его.
        MethodInfo exitMethod = GetStaticMethod(typeof(Environment), "Exit", [typeof(int)]);
        il.Emit(OpCodes.Call, exitMethod);
    }

    /// <summary>
    /// Находит статический метод указанного типа стандартной библиотеки классов .NET,
    ///  чтобы использовать его для реализации встроенной функции языка Tiger.
    /// </summary>
    private static MethodInfo GetStaticMethod(Type type, string methodName, Type[] parameterTypes)
    {
        MethodInfo? method = type.GetMethod(methodName, parameterTypes);
        if (method == null)
        {
            string parameterTypeNames = string.Join(", ", parameterTypes.Select(t => t.Name));
            throw new InvalidOperationException($"Cannot find method {type.Name}.{methodName}({parameterTypeNames}.");
        }

        return method;
    }
}