namespace PsTiger.Execution;

public interface IEnvironment
{
    /// <summary>
    /// Печатает текст в поток вывода.
    /// </summary>
    public void Print(string text);

    /// <summary>
    /// Печатает число в поток вывода.
    /// </summary>
    public void PrintInt(int value);

    /// <summary>
    /// Сбрасывает накопленный буфер вывода в поток вывода.
    /// </summary>
    public void Flush();
}