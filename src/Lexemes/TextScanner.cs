namespace PsTiger.Lexemes;

/// <summary>
///  Сканирует исходный код в виде строки, предоставляя три операции: Peek(N), Advance() и IsEnd().
/// </summary>
public class TextScanner
{
    private readonly string _input;
    private int _position;

    public TextScanner(string input)
    {
        _input = input;
    }

    /// <summary>
    ///  Читает на N символов вперёд текущей позиции (по умолчанию N=0).
    /// </summary>
    public char Peek(int n = 0)
    {
        int position = _position + n;
        return position >= _input.Length ? '\0' : _input[position];
    }

    /// <summary>
    ///  Сдвигает текущую позицию на один символ.
    /// </summary>
    public void Advance()
    {
        _position++;
    }

    /// <summary>
    /// Проверяет, достигли ли мы конца входных данных.
    /// </summary>
    public bool IsEnd()
    {
        return _position >= _input.Length;
    }
}