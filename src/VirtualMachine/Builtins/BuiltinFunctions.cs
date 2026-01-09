using PsTiger.Runtime;
using PsTiger.VirtualMachine.Exceptions;

namespace PsTiger.VirtualMachine.Builtins;

public class BuiltinFunctions
{
    private IEnvironment _environment;

    public BuiltinFunctions(IEnvironment environment)
    {
        _environment = environment;
    }

    public void Print(Value value)
    {
        _environment.Print(value.AsString());
    }

    public void PrintI(Value value)
    {
        _environment.PrintInt(value.AsInt());
    }

    public void Flush()
    {
        _environment.Flush();
    }

    public Value GetChar()
    {
        int code = _environment.ReadChar();
        if (code == -1)
        {
            return new Value("");
        }

        char c = (code < 128) ? (char)code : '?';
        return new Value(c.ToString());
    }

    public Value Ord(Value value)
    {
        string text = value.AsString();
        int result = (text.Length > 0) ? text[0] : -1;
        return new Value(result);
    }

    public Value Chr(Value value)
    {
        int code = value.AsInt();
        if (code < 0 || code >= 128)
        {
            throw new ProgramAbortedException($"Invalid character code {code}");
        }

        char ch = (char)code;
        return new Value(ch.ToString());
    }

    public Value Size(Value text)
    {
        return new Value(text.AsString().Length);
    }

    public Value Substring(Value value, Value fromIndex, Value length)
    {
        string text = value.AsString();

        // Разрешаем выход за границы строки, в этом случае результат будет короче заданной длины.
        int safeLength = int.Min(length.AsInt(), text.Length - fromIndex.AsInt());

        return new Value(text.Substring(fromIndex.AsInt(), safeLength));
    }

    public Value Concat(Value s1, Value s2)
    {
        return new Value(s1.AsString() + s2.AsString());
    }
}