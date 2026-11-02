using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Filee.App.Services;
using Filee.App.ViewModels;
using Filee.Core.Localization;
using Filee.Core.Presets;

namespace Filee.App.Views;

/// <summary>Choose a mode before each wheel-driven archive extraction, or configure archive packing.</summary>
public partial class ArchiveChoiceWindow : Window
{
    private readonly IReadOnlyList<string> _files;
    private readonly Preset _preset;
    private readonly ConversionService _conversions;
    private readonly HatchZipService _hatch;
    private readonly ILocalizer _loc;

    public ArchiveChoiceWindow(IReadOnlyList<string> files, Preset preset,
        ConversionService conversions, HatchZipService hatch, ILocalizer loc)
    {
        _files = files.ToList();
        _preset = preset;
        _conversions = conversions;
        _hatch = hatch;
        _loc = loc;
        DataContext = new ArchiveChoiceViewModel(_files, preset, hatch.IsAvailable, loc);
        InitializeComponent();
        Motion.Track(this);
        UiZoom.Track(this);
    }

    private void OnStart(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ArchiveChoiceViewModel vm || !vm.CanStart)
            return;
        string? destination = null;
        {
            try
            {
                destination = Path.GetFullPath(vm.Destination.Trim());
                if (!Directory.Exists(destination))
                    throw new DirectoryNotFoundException(vm.Destination);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or DirectoryNotFoundException or PathTooLongException)
            {
                vm.Status = _loc["winzlee.archive.destination_invalid"];
                return;
            }
        }
        if (!vm.IsExtraction || !vm.OptimizedSelected)
        {
            var effective = vm.IsCompression
                ? ArchiveChoicePolicy.ForCompression(_preset, vm.SelectedFormat.Value, vm.SelectedLevel.Value)
                : _preset.Clone();
            // Preserve existing output rules when the default folder is unchanged.
            // A custom destination applies to compression and classic extraction.
            var original = Path.GetDirectoryName(Path.GetFullPath(_files[0]))!;
            if (vm.IsCompression || !string.Equals(destination, original, StringComparison.OrdinalIgnoreCase))
                effective = AskedLocation.ForFolder(effective, destination!);
            _conversions.Start(_files, effective);
            Close();
            return;
        }

        // A separate checkbox in the dialog is mandatory for destructive extraction.
        if (!vm.Acknowledged || !_hatch.IsAvailable)
            return;
        // Confirmation is complete. The actual operation and its lifetime now belong to
        // the regular bottom-right toast, never to this chooser.
        _conversions.StartHatchZip(_hatch, _files, destination!);
        Close();
    }

    private async void OnBrowseDestination(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ArchiveChoiceViewModel { CanEdit: true } vm)
            return;
        var start = await StorageProvider.TryGetFolderFromPathAsync(vm.Destination);
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            SuggestedStartLocation = start,
        });
        if (folders.FirstOrDefault()?.TryGetLocalPath() is { } selected)
            vm.Destination = selected;
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();
}
