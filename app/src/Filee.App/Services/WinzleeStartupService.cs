// Winzlee-only Windows startup entry, independent of Filee's disabled shell integration.
// Registered only when the user explicitly enables it in General settings.
using Microsoft.Win32;

namespace Filee.App.Services;

internal static class WinzleeStartupService
{
    internal const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    internal const string EntryName = "Winzlee";

    internal static string CommandFor(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath)
            || !string.Equals(Path.GetFileName(executablePath), "Winzlee.exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Automatic startup requires Winzlee.exe.", nameof(executablePath));
        return "\"" + Path.GetFullPath(executablePath) + "\" --background";
    }

    public static bool IsEnabled(string? executablePath)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(executablePath))
            return false;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            var registered = key?.GetValue(EntryName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
            return string.Equals(registered, CommandFor(executablePath), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                                   System.Security.SecurityException or ArgumentException or NotSupportedException)
        {
            return false;
        }
    }

    public static void SetEnabled(bool enabled, string? executablePath)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows-only Winzlee startup entry.");
        if (enabled)
        {
            var command = CommandFor(executablePath ?? "");
            if (!File.Exists(executablePath))
                throw new FileNotFoundException("Winzlee.exe was not found.", executablePath);
            using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true)
                            ?? throw new IOException("Could not open the Windows startup registry key.");
            key.SetValue(EntryName, command, RegistryValueKind.String);
            if (!IsEnabled(executablePath))
                throw new IOException("Windows did not retain the Winzlee startup entry.");
        }
        else
        {
            // Never touch Filee or other software.
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            key?.DeleteValue(EntryName, throwOnMissingValue: false);
        }
    }
}
