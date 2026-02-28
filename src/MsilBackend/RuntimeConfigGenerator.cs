using System.Text.Json;
using System.Text.Json.Serialization;

namespace PsTiger.MsilBackend;

/// <summary>
/// Генерирует файл *.runtimeconfig.json для выбора подходящего фреймворка
///  при запуске программы на .NET в Linux / Mac OS X.
/// </summary>
/// <remarks>
/// Для запуска программ на .NET, зависимых от фреймворка (framework dependent), нужно правильно определить фреймворк.
/// </remarks>
public static class RuntimeConfigGenerator
{
    public static void SaveRuntimeConfig(string outputPath)
    {
        // Создаём *.runtimeconfig.json для запуска с той же мажорной версией .NET, на которой запущен сам компилятор.
        Version netVersion = Environment.Version;
        RuntimeConfig config = new($"net{netVersion.Major}.0", $"{netVersion.Major}.0.0");

        JsonSerializerOptions options = new()
        {
            WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        string json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(outputPath, json);
    }

    /// <summary>
    /// Объект "framework" в файле *.runtimeconfig.json.
    /// </summary>
    public class FrameworkInfo(string version)
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = "Microsoft.NETCore.App";

        [JsonPropertyName("version")]
        public string Version { get; init; } = version;
    }

    /// <summary>
    /// Объект "runtimeOptions" в файле *.runtimeconfig.json.
    /// </summary>
    public class RuntimeOptions(string targetFramework, string version)
    {
        [JsonPropertyName("tfm")]
        public string Tfm { get; init; } = targetFramework;

        [JsonPropertyName("framework")]
        public FrameworkInfo Framework { get; init; } = new(version);
    }

    /// <summary>
    /// Корневой объект в файле *.runtimeconfig.json.
    /// </summary>
    public class RuntimeConfig(string targetFramework, string version)
    {
        [JsonPropertyName("runtimeOptions")]
        public RuntimeOptions RuntimeOptions { get; init; } = new(targetFramework, version);
    }
}