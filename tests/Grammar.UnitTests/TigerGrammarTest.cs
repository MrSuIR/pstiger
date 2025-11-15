using System.Runtime.CompilerServices;

using Grammar;

using Xunit;

namespace Grammar.UnitTests;

public class TigerGrammarTest
{
    private static readonly string ValidProgramsDir = Path.Combine(GetTestDirectory(), "valid");
    private static readonly string InvalidProgramsDir = Path.Combine(GetTestDirectory(), "invalid");

    [Theory]
    [MemberData(nameof(GetValidPrograms))]
    public void Accepts_valid_programs(string path)
    {
        string code = File.ReadAllText(Path.Combine(ValidProgramsDir, path));
        TigerGrammar.CheckProgramSyntax(code);
    }

    [Theory]
    [MemberData(nameof(GetInvalidPrograms))]
    public void Rejects_invalid_programs(string path)
    {
        string code = File.ReadAllText(Path.Combine(InvalidProgramsDir, path));
        Assert.Throws<InvalidOperationException>(() => TigerGrammar.CheckProgramSyntax(code));
    }

    public static TheoryData<string> GetValidPrograms()
    {
        TheoryData<string> theoryData = [];
        foreach (string path in ListTigProgramPaths(ValidProgramsDir))
        {
            theoryData.Add(Path.GetFileName(path));
        }

        return theoryData;
    }

    public static TheoryData<string> GetInvalidPrograms()
    {
        TheoryData<string> theoryData = [];
        foreach (string path in ListTigProgramPaths(InvalidProgramsDir))
        {
            theoryData.Add(Path.GetFileName(path));
        }

        return theoryData;
    }

    private static string[] ListTigProgramPaths(string dir)
    {
        if (!Directory.Exists(dir))
        {
            throw new DirectoryNotFoundException($"Directory not found: {dir}");
        }

        string[] paths = Directory.GetFiles(dir, "*.tig");
        if (paths.Length < 1)
        {
            throw new InvalidOperationException($"No tig files found in {dir}");
        }

        return paths;
    }

    private static string GetTestDirectory([CallerFilePath] string path = "")
    {
        return Path.GetDirectoryName(path)
               ?? throw new ArgumentException($"Cannot get directory path from {path}");
    }
}