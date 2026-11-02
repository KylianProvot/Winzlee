// One lightweight PowerToys-style archive dialog, reused for unpacking and packing.
// Always asks extraction mode; a destructive HatchZip extraction requires an additional checkbox.
using CommunityToolkit.Mvvm.ComponentModel;
using Filee.App.Services;
using Filee.Core.Localization;
using Filee.Core.Presets;

namespace Filee.App.ViewModels;

public sealed partial class ArchiveChoiceViewModel : ObservableObject
{
    public ArchiveChoiceViewModel(IReadOnlyList<string> files, Preset preset, bool hatchAvailable, ILocalizer loc)
    {
        _loc = loc;
        IsExtraction = ArchiveChoicePolicy.IsExtraction(preset, files);
        HatchAvailable = hatchAvailable && files.All(ArchiveChoicePolicy.HatchZipCanRead);
        Heading = files.Count == 1 ? Path.GetFileName(files[0]) : loc.Format("winzlee.archive.count", files.Count);
        ExtractionHint = loc["winzlee.archive.extract_hint"];
        Warning = loc["winzlee.archive.danger"];
        FormatChoices = ArchiveChoicePolicy.PackFormats.Select(format =>
            new Choice<string>(format, format.ToUpperInvariant() == "TGZ" ? "TAR.GZ" : format.ToUpperInvariant())).ToList();
        SelectedFormat = FormatChoices.FirstOrDefault(c => c.Value == preset.TargetFormat) ?? FormatChoices[0];
        LevelChoices =
        [
            new Choice<ArchiveLevel>(ArchiveLevel.Store, loc["winzlee.archive.level.store"]),
            new Choice<ArchiveLevel>(ArchiveLevel.Fast, loc["winzlee.archive.level.fast"]),
            new Choice<ArchiveLevel>(ArchiveLevel.Normal, loc["winzlee.archive.level.normal"]),
            new Choice<ArchiveLevel>(ArchiveLevel.Maximum, loc["winzlee.archive.level.maximum"]),
        ];
        SelectedLevel = LevelChoices.First(l => l.Value == preset.Archive.Level);
        StartLabel = loc[IsExtraction ? "winzlee.archive.start_extract" : "winzlee.archive.start_compress"];
        Destination = Path.GetDirectoryName(Path.GetFullPath(files[0])) ?? Environment.CurrentDirectory;
    }

    public bool IsExtraction { get; }
    public bool IsCompression => !IsExtraction;
    public bool HatchAvailable { get; }
    public bool HatchUnavailable => IsExtraction && !HatchAvailable;
    public string Heading { get; }
    public string ExtractionHint { get; }
    public string Warning { get; }
    public string StartLabel { get; }
    public string DestinationLabel => _loc[IsExtraction ? "winzlee.archive.destination" : "winzlee.archive.compress_destination"];
    public IReadOnlyList<Choice<string>> FormatChoices { get; }
    public IReadOnlyList<Choice<ArchiveLevel>> LevelChoices { get; }
    private readonly ILocalizer _loc;
    public string FormatHelp => _loc["winzlee.archive.help." + (SelectedFormat?.Value ?? "zip")];

    [ObservableProperty] private Choice<string> _selectedFormat = null!;
    [ObservableProperty] private Choice<ArchiveLevel> _selectedLevel = null!;
    [ObservableProperty] private bool _optimizedSelected;
    [ObservableProperty] private bool _acknowledged;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _completed;
    [ObservableProperty] private string? _status;
    [ObservableProperty] private string _destination = "";

    public bool ClassicSelected
    {
        get => !OptimizedSelected;
        set { if (value) OptimizedSelected = false; }
    }

    public bool ShowAcknowledgement => IsExtraction && OptimizedSelected;
    public bool CanStart => !IsBusy && !Completed &&
                            (!IsExtraction || !OptimizedSelected || (HatchAvailable && Acknowledged));
    public bool CanEdit => !IsBusy && !Completed;
    public bool CanChooseHatch => HatchAvailable && CanEdit;

    partial void OnSelectedFormatChanged(Choice<string> value) => OnPropertyChanged(nameof(FormatHelp));

    partial void OnOptimizedSelectedChanged(bool value)
    {
        Acknowledged = false;
        OnPropertyChanged(nameof(ClassicSelected));
        OnPropertyChanged(nameof(ShowAcknowledgement));
        OnPropertyChanged(nameof(CanStart));
    }
    partial void OnAcknowledgedChanged(bool value) => OnPropertyChanged(nameof(CanStart));
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanChooseHatch));
    }
    partial void OnCompletedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanChooseHatch));
    }
}
