// Embedded HatchZip CLI adapter. The source archive is never modified without an explicit UI choice.
// v0.1.7 is packaged (checksum verified) by .github/workflows/winzlee-v09-archives.yml.
using System.ComponentModel;
using System.Diagnostics;
 
namespace Filee.App.Services;

public sealed class HatchZipService
{
    public string? Executable
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return null;
            var path = Path.Combine(AppContext.BaseDirectory, "engines", "hatchzip", "hatchzip.exe");
            return File.Exists(path) ? path : null;
        }
    }

    public bool IsAvailable => Executable is not null;

    /// <summary>Check all archives without changing them, then extract them one by one.</summary>
    public async Task ExtractSpaceSavingAsync(
        IReadOnlyList<string> archives, string destination, Action<string, string> status, CancellationToken cancellationToken)
    {
        var executable = Executable ?? throw new FileNotFoundException("HatchZip is not included in this build.");
        if (archives.Count == 0 || archives.Any(p => !File.Exists(p) || !ArchiveChoicePolicy.HatchZipCanRead(p)))
            throw new ArgumentException("Unsupported or missing archive file.", nameof(archives));

        // The check is read-only. Run for the whole selection before starting a destructive extraction.
        // HatchZip still does its own space check again before each extraction.
        foreach (var archive in archives)
        {
            cancellationToken.ThrowIfCancellationRequested();
            status("check", Path.GetFileName(archive));
            await RunCommandAsync(executable, ["check", archive], cancellationToken);
        }

        foreach (var archive in archives)
        {
            cancellationToken.ThrowIfCancellationRequested();
            status("extract", Path.GetFileName(archive));
            // The UI requires a separate positive acknowledgement before reaching this command.
            // --verify-first checks all entries before source modification; HatchZip safeguards resumption.
            var arguments = new List<string> { "extract", archive, "--verify-first", "--yes" };
            // Same-directory mode keeps HatchZip's original output naming and resume path.
            // For a chosen folder, one named subfolder per archive prevents collisions.
            var sourceFolder = Path.GetDirectoryName(Path.GetFullPath(archive))!;
            if (!Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar)
                    .Equals(sourceFolder.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            {
                arguments.Add("--output");
                arguments.Add(ArchiveChoicePolicy.HatchOutputDirectory(archive, destination));
            }
            await RunCommandAsync(executable, arguments, cancellationToken);
        }
        status("done", "");
    }

    private static async Task RunCommandAsync(
        string executable, IReadOnlyList<string> arguments, CancellationToken token)
    {
        var info = new ProcessStartInfo(executable)
        {
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(arguments[1]))!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument); // No quoting/shell injection, including names with spaces.
        using var process = Process.Start(info) ?? throw new IOException("HatchZip could not start.");
        using var registration = token.Register(() =>
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { }
            catch (Win32Exception) { }
        });
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(token);
        var output = await stdout;
        var errors = await stderr;
        token.ThrowIfCancellationRequested();
        if (process.ExitCode != 0)
        {
            var detail = (errors.Length > 0 ? errors : output).Trim();
            if (detail.Length > 1000) detail = detail[^1000..];
            throw new IOException($"HatchZip exited with code {process.ExitCode}: {detail}");
        }
    }
}
