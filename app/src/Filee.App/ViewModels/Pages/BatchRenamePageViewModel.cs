using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Filee.App.Services;
using Filee.App.ViewModels;
using Filee.Core.Conversion;
using Filee.Core.Formats;
using Filee.Core.Localization;
using Filee.Core.Platform;
using Filee.Core.Presets;

namespace Filee.App.ViewModels.Pages;

public sealed record BatchRenamePreviewRow(string Source, string OldName, string NewName,
    string Target, string? Error)
{
    public bool IsValid => Error is null;
}

/// <summary>A dedicated, PowerToys-style batch renaming workspace. No wheel/Explorer hooks.</summary>
public sealed partial class BatchRenamePageViewModel : ObservableObject, IDisposable
{
    private readonly ILocalizer _loc;
    private readonly ConverterCatalog _catalog;
    private readonly ConversionService _conversions;
    private readonly IPlatformServices _platform;
    private readonly List<string> _sources = [];
    private IReadOnlyList<BatchRenameItem> _plan = [];
    private readonly List<(string From, string To, long Bytes, DateTime Modified)> _undo = [];
    private CancellationTokenSource? _running;
    private bool _disposed;

    public BatchRenamePageViewModel(ILocalizer loc, ConverterCatalog catalog,
        ConversionService conversions, IPlatformServices platform)
    {
        _loc = loc;
        _catalog = catalog;
        _conversions = conversions;
        _platform = platform;
        SeparatorChoices =
        [
            new Choice<string>(" ", loc["batch.separator.space"]),
            new Choice<string>("-", loc["batch.separator.dash"]),
            new Choice<string>("_", loc["batch.separator.underscore"]),
            new Choice<string>("", loc["batch.separator.none"]),
        ];
        _selectedSeparator = SeparatorChoices[0];
        LetterCaseChoices =
        [
            new Choice<string>("keep", loc["batch.case.keep"]),
            new Choice<string>("lower", loc["batch.case.lower"]),
            new Choice<string>("upper", loc["batch.case.upper"])
        ];
        _selectedLetterCase = LetterCaseChoices[0];
        _formatChoices = [new Choice<string?>((string?)null, loc["batch.keep_formats"])];
        _selectedFormat = _formatChoices[0];
    }

    public ObservableCollection<BatchRenamePreviewRow> Preview { get; } = [];
    public IReadOnlyList<Choice<string>> SeparatorChoices { get; }
    [ObservableProperty] private string _baseName = "Photo";
    [ObservableProperty] private string _firstNumber = "1";
    [ObservableProperty] private Choice<string> _selectedSeparator;
    [ObservableProperty] private IReadOnlyList<Choice<string?>> _formatChoices;
    [ObservableProperty] private Choice<string?> _selectedFormat;
    [ObservableProperty] private bool _useOriginalNames;
    [ObservableProperty] private bool _addNumbers = true;
    [ObservableProperty] private string _findText = "";
    [ObservableProperty] private string _replaceText = "";
    [ObservableProperty] private string _prefix = "";
    [ObservableProperty] private string _suffix = "";
    [ObservableProperty] private Choice<string> _selectedLetterCase;
    public IReadOnlyList<Choice<string>> LetterCaseChoices { get; }
    [ObservableProperty] private string _destinationFolder = "";
    [ObservableProperty] private bool _deleteOriginals;
    [ObservableProperty] private bool _busy;
    [ObservableProperty] private int _completedCount;
    [ObservableProperty] private string _status = "";
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _canUndo;

    public bool HasFiles => _sources.Count != 0;
    public int FileCount => _sources.Count;
    public string FileCountText => _loc.Format("batch.count", FileCount);
    public bool IsConversion => SelectedFormat?.Value is not null;
    public bool CanDeleteOriginals => IsConversion && !Busy;
    public bool CanRun => !Busy && _plan.Count > 0 && _plan.All(p => p.IsValid);
    public bool CanClear => !Busy && HasFiles;
    public bool HasDestination => !string.IsNullOrWhiteSpace(DestinationFolder);
    public string DestinationDescription => HasDestination
        ? _loc["batch.destination_copy"] : _loc["batch.destination_source"];

    public void AddFiles(IEnumerable<string> paths)
    {
        if (Busy) return;
        var known = new HashSet<string>(_sources, OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var ignored = 0;
        foreach (var path in paths)
        {
            try
            {
                var full = Path.GetFullPath(path);
                if (File.Exists(full))
                {
                    if (known.Add(full)) _sources.Add(full);
                }
                else ignored++;
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                ignored++;
            }
        }
        _undo.Clear();
        CanUndo = false;
        UpdateFormats();
        Refresh();
        if (ignored > 0)
            Status = _loc.Format("batch.ignored", ignored);
    }

    /// <summary>Removes one entry from the selection only. Never touches files on disk.</summary>
    public void RemoveFile(string source)
    {
        if (Busy || string.IsNullOrWhiteSpace(source)) return;
        var index = _sources.FindIndex(path => BatchRenamePlan.SamePath(path, source));
        if (index < 0) return;
        _sources.RemoveAt(index);
        _undo.Clear();
        CanUndo = false;
        UpdateFormats();
        Status = "";
        Refresh();
    }

    [RelayCommand]
    private void Clear()
    {
        if (Busy) return;
        _sources.Clear();
        _undo.Clear();
        CanUndo = false;
        DeleteOriginals = false;
        DestinationFolder = "";
        UpdateFormats();
        Status = "";
        Refresh();
    }

    [RelayCommand]
    private void ClearDestination() => DestinationFolder = "";

    partial void OnBaseNameChanged(string value) => Refresh();
    partial void OnFirstNumberChanged(string value) => Refresh();
    partial void OnUseOriginalNamesChanged(bool value) => Refresh();
    partial void OnAddNumbersChanged(bool value) => Refresh();
    partial void OnFindTextChanged(string value) => Refresh();
    partial void OnReplaceTextChanged(string value) => Refresh();
    partial void OnPrefixChanged(string value) => Refresh();
    partial void OnSuffixChanged(string value) => Refresh();
    partial void OnSelectedLetterCaseChanged(Choice<string> value) => Refresh();
    partial void OnSelectedSeparatorChanged(Choice<string> value) => Refresh();
    partial void OnSelectedFormatChanged(Choice<string?> value)
    {
        DeleteOriginals = false;
        Refresh();
        OnPropertyChanged(nameof(IsConversion));
        OnPropertyChanged(nameof(CanDeleteOriginals));
    }
    partial void OnDestinationFolderChanged(string value)
    {
        Refresh();
        OnPropertyChanged(nameof(HasDestination));
        OnPropertyChanged(nameof(DestinationDescription));
    }
    partial void OnBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanRun));
        OnPropertyChanged(nameof(CanClear));
        OnPropertyChanged(nameof(CanDeleteOriginals));
    }

    private void UpdateFormats()
    {
        string? previous = SelectedFormat?.Value;
        var available = new List<Choice<string?>>
        {
            new(null, _loc["batch.keep_formats"])
        };
        if (_sources.Count > 0 && _sources.All(path => FormatRegistry.Detect(path) is not null))
        {
            var planner = _catalog.CreatePlanner();
            string[] preferred = ["png", "jpg", "webp", "pdf", "tiff", "bmp", "gif", "avif",
                "heic", "svg", "docx", "txt", "mp3", "wav", "flac", "mp4", "webm"];
            // Prefer familiar targets, but offer every other writable format that ALL sources
            // can actually reach with the currently available Winzlee engines.
            var candidates = preferred.Concat(FormatRegistry.Known
                .Where(f => f.Writable && f.Category != FormatCategory.Archive && f.Id != FormatRegistry.Folder)
                .Select(f => f.Id)).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var format in candidates)
            {
                if (FormatRegistry.FindById(format) is not { Writable: true } target ||
                    target.Category == FormatCategory.Archive || target.Id == FormatRegistry.Folder) continue;
                if (_sources.All(path =>
                {
                    var source = FormatRegistry.Detect(path)!.Id;
                    return source == target.Id || planner.Plan(source, target.Id) is not null;
                }))
                    available.Add(new Choice<string?>(target.Id, target.DisplayName));
            }
        }
        FormatChoices = available;
        SelectedFormat = available.FirstOrDefault(c => c.Value == previous) ?? available[0];
    }

    private void Refresh()
    {
        if (Busy) return;
        _plan = [];
        Preview.Clear();
        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(FileCount));
        OnPropertyChanged(nameof(FileCountText));
        // Computed from the selection, not a backing ObservableProperty: notify after every add/remove.
        OnPropertyChanged(nameof(CanClear));
        if (_sources.Count == 0)
        {
            OnPropertyChanged(nameof(CanRun));
            return;
        }
        if (!int.TryParse(FirstNumber, out var number) || number < 0 || number > 1_000_000_000)
        {
            Status = _loc["batch.invalid_number"];
            HasError = true;
            OnPropertyChanged(nameof(CanRun));
            return;
        }
        try
        {
            var planner = IsConversion ? _catalog.CreatePlanner() : null;
            _plan = BatchRenamePlan.Build(_sources,
                new BatchRenameOptions(BaseName, number, SelectedSeparator?.Value ?? " ",
                    SelectedFormat?.Value, string.IsNullOrWhiteSpace(DestinationFolder) ? null : DestinationFolder.Trim(),
                    UseOriginalNames, AddNumbers, FindText, ReplaceText, Prefix, Suffix,
                    SelectedLetterCase?.Value ?? "keep"),
                (from, to) => planner?.Plan(from, to) is not null);
            foreach (var p in _plan)
                Preview.Add(new BatchRenamePreviewRow(p.Source, p.OriginalName, p.NewName,
                    p.Destination, p.Error is null ? null : _loc["batch.error." + p.Error]));
            HasError = _plan.Any(p => !p.IsValid);
            Status = HasError ? _loc["batch.fix_errors"] : "";
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            Status = _loc.Format("batch.failed", ex.Message);
            HasError = true;
        }
        OnPropertyChanged(nameof(CanRun));
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        if (!CanRun || _disposed) return;
        // Re-evaluate filesystem collisions immediately before any write.
        Refresh();
        if (!CanRun) return;
        var work = _plan.ToArray();
        var convert = IsConversion;
        var remove = convert && DeleteOriginals;
        var originals = new List<(string Source, long Length, DateTime LastWriteTimeUtc)>();
        var movedToTrash = new List<string>();
        string PartialTrashStatus(string reason)
        {
            var moved = new HashSet<string>(movedToTrash, StringComparer.OrdinalIgnoreCase);
            var remaining = originals.Where(o => !moved.Contains(o.Source)).Select(o => Path.GetFileName(o.Source));
            return _loc.Format("batch.trash_partial", movedToTrash.Count,
                string.Join(", ", movedToTrash.Select(Path.GetFileName)),
                string.Join(", ", remaining), reason);
        }
        var cancellation = _running = new CancellationTokenSource();
        Busy = true;
        HasError = false;
        CompletedCount = 0;
        _undo.Clear();
        CanUndo = false;
        try
        {
            originals = work.Select(item =>
            {
                var info = new FileInfo(item.Source);
                return (item.Source, info.Length, info.LastWriteTimeUtc);
            }).ToList();
            if (!convert)
            {
                var changed = new List<(string From, string To, long Bytes, DateTime Modified)>();
                try
                {
                    foreach (var item in work)
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        if (item.ChangesPath)
                        {
                            if (File.Exists(item.Destination)) throw new IOException(_loc["batch.error.exists"]);
                            if (HasDestination)
                                await Task.Run(() => File.Copy(item.Source, item.Destination), cancellation.Token);
                            else
                                await Task.Run(() => BatchRenamePlan.MoveFileExact(item.Source, item.Destination), cancellation.Token);
                            var info = new FileInfo(item.Destination);
                            changed.Add((item.Source, item.Destination, info.Length, info.LastWriteTimeUtc));
                        }
                        CompletedCount++;
                        Status = _loc.Format("batch.progress", CompletedCount, work.Length);
                    }
                }
                catch
                {
                    // In-place renaming is reversible. Never undo a copied result behind the user's back.
                    if (!HasDestination)
                        foreach (var item in changed.AsEnumerable().Reverse())
                        {
                            if (File.Exists(item.To) && !File.Exists(item.From))
                                try { BatchRenamePlan.MoveFileExact(item.To, item.From); } catch (IOException) { }
                        }
                    throw;
                }
                if (!HasDestination)
                {
                    _undo.AddRange(changed);
                    CanUndo = _undo.Count > 0;
                }
            }
            else
            {
                foreach (var item in work)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    if (File.Exists(item.Destination)) throw new IOException(_loc["batch.error.exists"]);
                    if (item.SourceFormat == item.TargetFormat)
                    {
                        // Already in the requested format: preserve the bytes without needless re-encoding.
                        await Task.Run(() => File.Copy(item.Source, item.Destination), cancellation.Token);
                    }
                    else
                    {
                        var preset = new Preset
                        {
                            Id = "winzlee-batch-rename",
                            Name = "Winzlee",
                            TargetFormat = item.TargetFormat!,
                        };
                        preset.Output.Location = OutputLocation.CustomFolder;
                        preset.Output.CustomFolder = Path.GetDirectoryName(item.Destination)!;
                        preset.Output.FileNamePattern = Path.GetFileNameWithoutExtension(item.Destination);
                        preset.Output.Conflict = ConflictPolicy.Skip;
                        preset.Output.AskEveryTime = false;
                        preset.Output.ReplaceOriginal = false;
                        preset.Output.OnlyIfSmaller = false;
                        var job = await _conversions.RunAsync([item.Source], preset, cancellationToken: cancellation.Token);
                        cancellation.Token.ThrowIfCancellationRequested();
                        if (job is null || job.State != JobState.Completed ||
                            job.Files.Count != 1 || job.Files[0].State != FileState.Done ||
                            job.Files[0].Outputs.Count != 1 ||
                            !BatchRenamePlan.SamePath(job.Files[0].Outputs[0], item.Destination))
                            throw new IOException(_loc["batch.conversion_incomplete"]);
                    }
                    var output = new FileInfo(item.Destination);
                    if (!output.Exists || output.Length == 0)
                        throw new IOException(_loc["batch.conversion_incomplete"]);
                    CompletedCount++;
                    Status = _loc.Format("batch.progress", CompletedCount, work.Length);
                }
                // Two-phase safety: do not trash a *single* original until every output exists
                // and is verified, and no original has changed during the conversion.
                if (remove)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    foreach (var item in work)
                        if (!File.Exists(item.Destination) || new FileInfo(item.Destination).Length == 0)
                            throw new IOException(_loc["batch.conversion_incomplete"]);
                    foreach (var original in originals)
                    {
                        var current = new FileInfo(original.Source);
                        if (!current.Exists || current.Length != original.Length ||
                            current.LastWriteTimeUtc != original.LastWriteTimeUtc)
                            throw new IOException(_loc["batch.original_changed"]);
                    }
                    foreach (var original in originals)
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        // Recheck immediately before moving this original: it could have changed since preflight.
                        var current = new FileInfo(original.Source);
                        if (!current.Exists || current.Length != original.Length ||
                            current.LastWriteTimeUtc != original.LastWriteTimeUtc)
                            throw new IOException(_loc["batch.original_changed"]);
                        var refusal = await Task.Run(() => _platform.MoveToTrash(original.Source));
                        if (refusal is not null)
                            throw new IOException(_loc.Format("batch.trash_failed", Path.GetFileName(original.Source)));
                        movedToTrash.Add(original.Source);
                    }
                }
            }
            Status = remove ? _loc["batch.done_trashed"] : _loc["batch.done"];
        }
        catch (OperationCanceledException)
        {
            HasError = true;
            Status = movedToTrash.Count > 0
                ? PartialTrashStatus(_loc["batch.trash_cancelled"])
                : _loc["batch.cancelled"];
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            HasError = true;
            Status = movedToTrash.Count > 0
                ? PartialTrashStatus(ex.Message)
                : _loc.Format("batch.failed", ex.Message);
        }
        finally
        {
            Busy = false;
            _running = null;
            cancellation.Dispose();
            OnPropertyChanged(nameof(CanRun));
        }
    }

    [RelayCommand]
    private void Cancel() => _running?.Cancel();

    [RelayCommand]
    private async Task UndoAsync()
    {
        if (!CanUndo || Busy) return;
        Busy = true;
        try
        {
            await Task.Run(() =>
            {
                // Reverse order also handles names that could become occupied by another undo.
                foreach (var item in _undo.AsEnumerable().Reverse())
                {
                    var current = new FileInfo(item.To);
                    if (!current.Exists || (File.Exists(item.From) && !BatchRenamePlan.SamePath(item.From, item.To)) ||
                        current.Length != item.Bytes || current.LastWriteTimeUtc != item.Modified)
                        throw new IOException(_loc["batch.undo_changed"]);
                    BatchRenamePlan.MoveFileExact(item.To, item.From);
                }
            });
            _undo.Clear();
            CanUndo = false;
            Status = _loc["batch.undone"];
            HasError = false;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Status = _loc.Format("batch.failed", ex.Message);
            HasError = true;
        }
        finally
        {
            Busy = false;
            Refresh();
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _running?.Cancel(); // Only on actual session teardown, never on normal page navigation.
    }
}
