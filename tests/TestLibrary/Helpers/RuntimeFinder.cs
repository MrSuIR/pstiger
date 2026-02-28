using System.Runtime.InteropServices;

namespace PsTiger.Tests.TestLibrary.Helpers;

/// <summary>
/// Выполняет поиск рантайма .NET в текущей системе.
/// </summary>
public class RuntimeFinder
{
    /// <summary>
    /// Имя рантайма для запуска консольных приложений.
    /// Такой рантайм является частью фреймворка .NET и служит для запуска простых приложений, не
    ///   являющихся ни сервисами на ASP.NET Core, ни десктопными приложениями для Windows.
    /// </summary>
    private const string AppRuntimeName = "Microsoft.NETCore.App";

    /// <summary>
    /// Возвращает шаблон пути к базовым библиотекам фреймворка .NET,
    ///  используемых для консольных приложений.
    /// Пример: "/usr/lib/dotnet/shared/Microsoft.NETCore.App/10.0.1/*.dll".
    /// </summary>
    /// <remarks>
    /// Используется версия .NET, на которой запущен текущий процесс.
    /// </remarks>
    public static string GetRuntimeLibrariesPathPattern()
    {
        string root = GetDotnetRootPath();

        // Получаем версию текущего рантайма.
        string runtimeVersion = Environment.Version.ToString();

        // Формируем путь к каталогу с библиотеками фреймворка .NET.
        string librariesDir = Path.Combine(root, "shared", AppRuntimeName, runtimeVersion);

        if (!Directory.Exists(librariesDir))
        {
            throw new NotSupportedException(
                $".NET runtime runtime libraries directory '{librariesDir}' does not exists.");
        }

        // Формируем шаблон пути к библиотекам фреймворка .NET.
        return Path.Combine(librariesDir, "*.dll");
    }

    private static string GetDotnetRootPath()
    {
        string? root = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(root))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                root = "/usr/local/share/dotnet";
            }
            else
            {
                root = "/usr/lib/dotnet";
            }
        }

        if (!Directory.Exists(root))
        {
            throw new NotSupportedException($"Cannot find .NET runtime root directory: '{root}' does not exists.");
        }

        return root;
    }
}