using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Filee.Core.Localization;

namespace Filee.App.ViewModels;

/// <summary>One consented HatchZip extraction shown alongside the normal conversion toast jobs.</summary>
public sealed partial class HatchZipToastViewModel : ObservableObject, IDisposable
{
    private readonly ILocalizer _loc;
    private readonly Action<HatchZipToastViewModel> _dismiss;
    private readonly CancellationTokenSource _cancellation = new();

    public HatchZipToastViewModel(IReadOnlyList<string> archives, ILocalizer loc,
        Action<HatchZipToastViewModel> dismiss)
    {
        _loc = loc;
        _dismiss = dismiss;
        Title = archives.Count == 1 ? Path.GetFileName(archives[0]) : loc.Format("winzlee.archive.count", archives.Count);
        Status = loc["winzlee.archive.checking"];
    }

    public string Title { get; }
    public CancellationToken CancellationToken => _cancellation.Token;

    [ObservableProperty] private string _status = "";
    [ObservableProperty] private string? _errorDetails;
    [ObservableProperty] private bool _isRunning = true;
    [ObservableProperty] private bool _isIndeterminate = true;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private bool _hasErrors;

    [RelayCommand]
    private void Cancel()
    {
        if (!IsRunning || _cancellation.IsCancellationRequested)
            return;
        Status = _loc["winzlee.archive.stopping"];
        _cancellation.Cancel();
    }

    [RelayCommand]
    private void Dismiss()
    {
        if (IsRunning)
            Cancel(); // Never hide an operation that may already be modifying the source archive.
        else
            _dismiss(this);
    }

    public void Report(string stage, string filename)
    {
        if (IsRunning && !_cancellation.IsCancellationRequested)
            Status = _loc.Format(stage == "check" ? "winzlee.archive.checking_file"
                : stage == "extract" ? "winzlee.archive.extracting_file" : "winzlee.archive.finished", filename);
    }

    public void Complete()
    {
        IsRunning = false;
        IsIndeterminate = false;
        Progress = 100;
        Status = _loc["winzlee.archive.finished"];
    }

    public void Interrupted()
    {
        IsRunning = false;
        IsIndeterminate = false;
        Status = _loc["winzlee.archive.interrupted"];
        HasErrors = true;
        ErrorDetails = _loc["winzlee.archive.danger"];
    }

    public void Failed(Exception ex)
    {
        IsRunning = false;
        IsIndeterminate = false;
        Status = _loc.Format("winzlee.archive.error", ex.Message);
        HasErrors = true;
        ErrorDetails = _loc["winzlee.archive.interrupted"];
    }

    public void Dispose() => _cancellation.Dispose();
}
