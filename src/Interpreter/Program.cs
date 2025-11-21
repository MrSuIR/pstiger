namespace PsTiger.Interpreter;

public static class Program
{
    public static int Main(string[] args)
    {
        // Проверяем, что передан путь к файлу с исходным кодом.
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: PsTiger.Interpreter <file-path>");
            return 1;
        }

        string sourcePath = args[0];
        if (!File.Exists(sourcePath))
        {
            Console.Error.WriteLine($"Error: source file '{sourcePath}' not found.");
            return 1;
        }

        try
        {
            string sourceCode = File.ReadAllText(sourcePath);

            // Выполняем программу
            ConsoleEnvironment environment = new();
            TigerInterpreter interpreter = new(environment);
            interpreter.Execute(sourceCode);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Interpreter threw an {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }
}