using System.Globalization;
using System.Text;

using PsTiger.Execution;

namespace Interpreter.IntegrationTests.TestDoubles;

public class FakeEnvironment : IEnvironment
{
    private readonly StringBuilder _outputBuffer = new();
    private readonly StringBuilder _flushedOutput = new();

    public string BufferedOutput => _outputBuffer.ToString();

    public string FlushedOutput => _flushedOutput.ToString();

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