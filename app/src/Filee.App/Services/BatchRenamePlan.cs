// Safe, filesystem-aware preview shared by the batch rename page and its executor.
using Filee.Core.Formats;

namespace Filee.App.Services;

public sealed record BatchRenameOptions(string BaseName, int FirstNumber, string Separator,
    string? TargetFormat, string? Destination = null, bool UseOriginalNames = false,
    bool AddNumbers = true, string Find = "", string Replace = "",
    string Prefix = "", string Suffix = "", string LetterCase = "keep");

public sealed record BatchRenameItem(string Source, string Destination, string OriginalName, string NewName,
    string? SourceFormat, string? TargetFormat, string? Error)
{
    public bool IsValid => Error is null;
    public bool ChangesPath => !string.Equals(Path.GetFullPath(Source),
        Path.GetFullPath(Destination), StringComparison.Ordinal);
}

public static class BatchRenamePlan
{
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    private static StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static bool SamePath(string left, string right) =>
        PathComparer.Equals(Path.GetFullPath(left), Path.GetFullPath(right));

    /// <summary>Preserve case-only Windows renames, without overwriting anything.</summary>
    public static void MoveFileExact(string source, string destination)
    {
        if (SamePath(source, destination) &&
            !string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.Ordinal))
        {
            var folder = Path.GetDirectoryName(source)!;
            var staging = Path.Combine(folder, ".winzlee-rename-" + Guid.NewGuid().ToString("N") + ".tmp");
            File.Move(source, staging);
            try { File.Move(staging, destination); }
            catch
            {
                if (File.Exists(staging) && !File.Exists(source))
                    File.Move(staging, source);
                throw;
            }
        }
        else
            File.Move(source, destination);
    }

    public static bool ValidStem(string stem) =>
        !string.IsNullOrWhiteSpace(stem) && stem == stem.TrimEnd(' ', '.') &&
        stem.IndexOfAny(new[] { '<', '>', ':', (char)34, '/', (char)92, '|', '?', '*' }) < 0 &&
        !stem.Any(char.IsControl) && !ReservedNames.Contains(stem.Split('.')[0]) &&
        stem.Length <= 180;

    public static IReadOnlyList<BatchRenameItem> Build(IReadOnlyList<string> sources, BatchRenameOptions options,
        Func<string, string, bool>? canConvert = null)
    {
        var result = new List<BatchRenameItem>();
        var paths = new HashSet<string>(PathComparer);
        foreach (var source in sources)
        {
            var path = Path.GetFullPath(source);
            if (!paths.Add(path)) continue;
            var originalName = Path.GetFileName(path);
            var sourceFormat = FormatRegistry.Detect(path)?.Id;
            var originalExt = FormatRegistry.ExtensionOf(path);
            var ext = options.TargetFormat is null
                ? (originalExt.Length == 0 ? "" : "." + originalExt)
                : "." + (FormatRegistry.FindById(options.TargetFormat)?.Extensions.FirstOrDefault() ?? options.TargetFormat);
            var index = result.Count;
            var number = (long)options.FirstNumber + index;
            var baseStem = options.UseOriginalNames
                ? FormatRegistry.NameWithoutExtension(path) : options.BaseName.Trim();
            if (options.Find.Length > 0)
                baseStem = baseStem.Replace(options.Find, options.Replace, StringComparison.OrdinalIgnoreCase);
            baseStem = options.Prefix + baseStem + options.Suffix;
            baseStem = options.LetterCase switch
            {
                "lower" => baseStem.ToLowerInvariant(),
                "upper" => baseStem.ToUpperInvariant(),
                _ => baseStem
            };
            var stem = baseStem + (options.AddNumbers ? options.Separator + number : "");
            var newName = stem + ext;
            var folder = string.IsNullOrWhiteSpace(options.Destination)
                ? Path.GetDirectoryName(path)! : Path.GetFullPath(options.Destination);
            var target = Path.Combine(folder, newName);
            string? error = null;
            if (!File.Exists(path)) error = "missing";
            else if (!Directory.Exists(folder)) error = "folder";
            else if (!ValidStem(stem)) error = "name";
            else if (options.TargetFormat is not null && (sourceFormat is null ||
                     sourceFormat != options.TargetFormat && canConvert?.Invoke(sourceFormat, options.TargetFormat) != true))
                error = "unsupported";
            else if (options.TargetFormat is not null && SamePath(path, target))
                error = "same-path";
            result.Add(new BatchRenameItem(path, target, originalName, newName,
                sourceFormat, options.TargetFormat, error));
        }

        var duplicated = result.GroupBy(item => item.Destination, PathComparer)
            .Where(group => group.Count() > 1).SelectMany(group => group).ToHashSet();
        return result.Select(item =>
        {
            var error = item.Error;
            if (error is null && duplicated.Contains(item)) error = "duplicate";
            // Never overwrite any existing file, including a *different* source in this selection.
            if (error is null && !SamePath(item.Source, item.Destination) &&
                (File.Exists(item.Destination) || Directory.Exists(item.Destination)))
                error = "exists";
            return item with { Error = error };
        }).ToList();
    }
}
