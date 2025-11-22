using System.Globalization;
using System.Text;

using PsTiger.Execution;

namespace Interpreter.IntegrationTests.TestDoubles;

/// <summary>
/// Имитирует средства ввода-вывода для тестов.
/// </summary>
public class FakeEnvironment : IEnvironment
{
    private readonly Queue<char> _input = new();
    private readonly StringBuilder _outputBuffer = new();
    private readonly StringBuilder _flushedOutput = new();

    public string BufferedOutput => _outputBuffer.ToString();

    public string FlushedOutput => _flushedOutput.ToString();

    public void AddInput(string text)
    {
        foreach (char c in text)
        {
            _input.Enqueue(c);
        }
    }

    public int ReadChar()
    {
        if (_input.TryDequeue(out char c))
        {
            return c;
        }

        return -1;
    }

    public void Print(string text)
    {
        _outputBuffer.Append(text);
    }

    public void PrintInt(int value)
    {
        _outputBuffer.Append(value.ToString(CultureInfo.InvariantCulture));
    }

    public void Flush()
    {
        _flushedOutput.Append(_outputBuffer.ToString());
        _outputBuffer.Clear();
    }
}