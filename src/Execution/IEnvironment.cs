namespace PsTiger.Execution;

public interface IEnvironment
{
    /// <summary>
    /// Читает символ из потока ввода либо возвращает -1, если достигнут конец файла.
    /// </summary>
    public int ReadChar();

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