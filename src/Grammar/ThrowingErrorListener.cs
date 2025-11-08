using Antlr4.Runtime;

namespace Grammar;

/// <summary>
/// Обработчик ошибок ANTLR4, который сохраняет информацию об ошибках синтаксиса.
/// Предоставляет информативные сообщения об ошибках синтаксиса.
/// </summary>
internal class ThrowingErrorListener : BaseErrorListener
{
    public bool HasError { get; private set; }

    public string ErrorMessage { get; private set; } = string.Empty;

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e
    )
    {
        // Формируем более информативное сообщение об ошибке
        string errorMessage;

        if (offendingSymbol != null)
        {
            // Если есть информация о проблемном токене, включаем её в сообщение
            string tokenText = offendingSymbol.Text ?? "<unknown>";
            string tokenType = recognizer.Vocabulary.GetSymbolicName(offendingSymbol.Type) ?? "<unknown>";

            errorMessage = $"Syntax error at line {line}, column {charPositionInLine}: {msg}. " +
                          $"Unexpected token '{tokenText}' of type {tokenType}";
        }
        else
        {
            // Если информации о токене нет, используем базовое сообщение
            errorMessage = $"Syntax error at line {line}, column {charPositionInLine}: {msg}";
        }

        // Сохраняем информацию об ошибке и бросаем исключение
        HasError = true;
        ErrorMessage = errorMessage;
        throw new RecognitionException(errorMessage, recognizer, null, null);
    }
}