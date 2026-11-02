// Chooser at the wheel boundary: normal background conversions and watch folders remain unattended.
using Filee.Core.Formats;
using Filee.Core.Presets;

namespace Filee.App.Services;

public static class ArchiveChoicePolicy
{
    // Formats whose compression level can be adjusted by our archive dialog.
    public static readonly IReadOnlyList<string> PackFormats = ["zip", "7z", "tar", "tgz"];

    public static bool IsExtraction(Preset preset, IReadOnlyList<string> sources) =>
        !preset.IsTool && !preset.IsGroup && preset.TargetFormat == FormatRegistry.Folder
        && sources.Count > 0 && sources.All(path =>
            !Directory.Exists(path) && FormatRegistry.Detect(path)?.Category == FormatCategory.Archive);

    public static bool IsCompression(Preset preset) =>
        !preset.IsTool && !preset.IsGroup && PackFormats.Contains(preset.TargetFormat);

    // Only the wheel asks. Watch folders, web downloads and other automatic tasks never get a modal prompt.
    public static bool ShouldPrompt(Preset preset, IReadOnlyList<string> sources, bool browserDrop) =>
        !browserDrop && (IsExtraction(preset, sources) || IsCompression(preset));

    // HatchZip's safe shrinking extraction supports these containers; not ALZ/EGG/other Filee formats.
    public static bool HatchZipCanRead(string path)
    {
        var name = Path.GetFileName(path);
        if (name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".7z.001", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".zip.001", StringComparison.OrdinalIgnoreCase))
            return true;
        return new[] { ".zip", ".7z", ".rar", ".tar", ".001" }
            .Any(extension => name.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Place each HatchZip archive in its own named directory within a custom destination.</summary>
    /// <remarks>The default location uses HatchZip's built-in naming, including recovery metadata.</remarks>
    public static string HatchOutputDirectory(string archive, string destination)
    {
        var name = Path.GetFileName(archive);
        foreach (var extension in new[] { ".tar.gz", ".7z.001", ".zip.001", ".tgz", ".zip", ".7z", ".rar", ".tar", ".001" })
        {
            if (name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^extension.Length];
                break;
            }
        }
        return Path.Combine(destination, name);
    }

    public static Preset ForCompression(Preset original, string targetFormat, ArchiveLevel level)
    {
        if (!PackFormats.Contains(targetFormat))
            throw new ArgumentOutOfRangeException(nameof(targetFormat), "Unsupported compression format.");
        var copy = original.Clone();
        copy.TargetFormat = targetFormat;
        copy.Archive.Level = level;
        return copy;
    }
}
