using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Grammar;

public static class TigerGrammar
{
    /// <summary>
    /// Проверяет текст программы на соответствие грамматике языка Tiger.
    /// Бросает исключение при несоответствии.
    /// </summary>
    public static void CheckProgramSyntax(string code)
    {
        // Создаём лексический анализатор, сгенерированный из грамматики `TigerLexer.g4`.
        AntlrInputStream inputStream = new(code);
        TigerLexer lexer = new(inputStream);

        // Создаём синтаксический анализатор, сгенерированный из грамматики `TigerParser.g4`
        CommonTokenStream tokenStream = new(lexer);
        TigerParser parser = new(tokenStream);

        // Создаем специальный обработчик ошибок, который будет сохранять информацию об ошибках
        ThrowingErrorListener errorListener = new ThrowingErrorListener();

        // Устанавливаем обработчик ошибок для парсера
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        // Используем DefaultErrorStrategy, но с настройкой, которая предотвращает восстановление после ошибок
        parser.ErrorHandler = new DefaultErrorStrategy();

        // Запускаем разбор по правилу `program` и проверяем, что весь входной текст был разобран
        try
        {
            TigerParser.ProgramContext programContext = parser.program();

            // Проверяем, что весь входной текст был разобран (достигнут конец файла)
            if (programContext.exception != null)
            {
                throw new Exception($"Syntax error: {programContext.exception.Message}");
            }

            // Дополнительная проверка: убедимся, что все токены были обработаны
            tokenStream.Fill();
            if (tokenStream.Size > 0 && tokenStream.Get(tokenStream.Size - 1).Type != TokenConstants.EOF)
            {
                IToken nextToken = tokenStream.Get(0);
                throw new Exception($"Unexpected token '{nextToken.Text}' at line {nextToken.Line}, column {nextToken.Column}. Expected end of input.");
            }
        }
        catch (ParseCanceledException ex)
        {
            // BailErrorStrategy бросает ParseCanceledException при ошибках
            // Используем сохраненное сообщение об ошибке из нашего обработчика
            if (errorListener.HasError)
            {
                throw new Exception(errorListener.ErrorMessage);
            }
            else
            {
                throw new Exception($"Syntax error: {ex.Message}", ex);
            }
        }
        catch (RecognitionException ex)
        {
            // Перехватываем исключения от нашего ThrowingErrorListener
            throw new Exception(ex.Message, ex);
        }
    }
}