// Read-only health checks: do not export personal paths or machine identifiers.
namespace Filee.App.Services;

internal sealed record WinzleeHealthSnapshot(
    bool IsWindows, bool DataFolderExists, bool SevenZipPresent, bool HatchZipPresent, bool StartupEnabled);

internal static class WinzleeHealthCheck
{
    internal static WinzleeHealthSnapshot Scan(string appDirectory, string dataDirectory, bool isWindows, bool startupEnabled)
    {
        bool Exists(string path) => File.Exists(Path.Combine(appDirectory, path));
        return new WinzleeHealthSnapshot(
            isWindows,
            Directory.Exists(dataDirectory),
            isWindows && Exists(Path.Combine("engines", "7zip", "7z.exe"))
                      && Exists(Path.Combine("engines", "7zip", "7z.dll")),
            isWindows && Exists(Path.Combine("engines", "hatchzip", "hatchzip.exe")),
            isWindows && startupEnabled);
    }
}
