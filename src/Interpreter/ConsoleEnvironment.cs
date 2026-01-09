using System.Globalization;

using PsTiger.VirtualMachine;

namespace PsTiger.Interpreter;

public class ConsoleEnvironment : IEnvironment
{
    public int ReadChar()
    {
        return Console.Read();
    }

    public void Print(string text)
    {
        Console.Write(text);
    }

    public void PrintInt(int value)
    {
        Console.Write(value.ToString(CultureInfo.InvariantCulture));
    }

    public void Flush()
    {
        Console.Out.Flush();
    }
}