using System.Runtime.CompilerServices;

namespace PsTiger.Tests.TestLibrary;

public static class Samples
{
    private const string DataDirectory = "data";

    public static string GetSampleProgram(string filename)
    {
        string filepath = Path.Join(
            GetClassDirectory(),
            DataDirectory,
            filename.Replace('/', Path.DirectorySeparatorChar)
        );
        return File.ReadAllText(filepath);
    }

    private static string GetClassDirectory([CallerFilePath] string path = "")
    {
        return Path.GetDirectoryName(path) ?? throw new ArgumentException($"Could not get directory path from {path}");
    }
}