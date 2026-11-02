// Main (settings) window: left navigation + current page.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Filee.App.Services;
using Filee.App.ViewModels.Pages;
using Filee.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Filee.App.ViewModels;

/// <summary>Navigation entry. <see cref="Title"/> follows the UI language.</summary>
public sealed partial class NavItem(string key, string titleKey, string iconKey, Type pageType) : ObservableObject
{
    public string Key { get; } = key;
    public string TitleKey { get; } = titleKey;
    public Geometry? Icon { get; } = Application.Current?.FindResource(iconKey) as Geometry;
    public Type PageType { get; } = pageType;

    [ObservableProperty] private string _title = "";

    public void Refresh(ILocalizer loc) => Title = loc[TitleKey];
}

/// <summary>
/// One per main window (<see cref="WindowService.ShowMain"/>); disposed with it, which drops its subscriptions on the
/// long-lived services and the current page.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILocalizer _loc;
    private bool _disposed;

    /// <summary>The repository, for the GitHub button above the version.</summary>
    public string RepositoryUrl => UpdateService.RepositoryUrl;

    public MainWindowViewModel(IServiceProvider services, ILocalizer loc, UpdateService updates)
    {
        _services = services;
        _loc = loc;
        Updates = updates;
        updates.PropertyChanged += OnUpdatesChanged;
        RefreshVersionTexts();
        Items =
        [
            new("home", "nav.home", "Icon.Home", typeof(HomePageViewModel)),
            new("general", "nav.general", "Icon.Settings", typeof(GeneralPageViewModel)),
            new("triggers", "nav.triggers", "Icon.Keyboard", typeof(TriggersPageViewModel)),
            new("toolbar", "nav.toolbar", "Icon.Donut", typeof(ToolbarPageViewModel)),
            new("presets", "nav.presets", "Icon.Tune", typeof(PresetsPageViewModel)),
            new("rename", "nav.rename", "Icon.Pencil", typeof(BatchRenamePageViewModel)),
            new("watch", "nav.watch", "Icon.Folder", typeof(WatchFoldersPageViewModel)),
            new("theme", "nav.theme", "Icon.Palette", typeof(ThemePageViewModel)),
            new("engines", "nav.engines", "Icon.Engine", typeof(EnginesPageViewModel)),
            new("about", "nav.about", "Icon.Info", typeof(AboutPageViewModel)),
        ];
        foreach (var item in Items)
            item.Refresh(loc);
        // Keep all routes for existing links/tests, but simplify visible navigation.
        PrimaryItems = Items.Where(i => i.Key is "home" or "triggers" or "toolbar" or "presets" or "rename").ToList();
        loc.LanguageChanged += OnLanguageChanged;
        _selectedItem = Items[0];
        _currentPage = CreatePage(Items[0]);
    }

    public IReadOnlyList<NavItem> Items { get; }
    public IReadOnlyList<NavItem> PrimaryItems { get; }

    // A PowerToys-style hamburger reduces navigation to a thin icon rail.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavigationCollapsed))]
    [NotifyPropertyChangedFor(nameof(SidebarWidth))]
    private bool _isNavigationExpanded = true;

    public bool IsNavigationCollapsed => !IsNavigationExpanded;
    public double SidebarWidth => IsNavigationExpanded ? 226 : 72;

    [RelayCommand]
    private void ToggleNavigation() => IsNavigationExpanded = !IsNavigationExpanded;

    public NavItem? SelectedPrimaryItem
    {
        get => PrimaryItems.Contains(SelectedItem) ? SelectedItem : null;
        set
        {
            if (value is not null && SelectedItem != value)
                SelectedItem = value;
        }
    }

    public bool IsGeneralSelected => SelectedItem.Key == "general";
    public bool IsAboutSelected => SelectedItem.Key == "about";

    [RelayCommand]
    private void OpenGeneral() => Navigate("general");

    [RelayCommand]
    private void OpenAbout() => Navigate("about");

    /// <summary>Update state for the bottom of the navigation (version, "update available" button).</summary>
    public UpdateService Updates { get; }

    [ObservableProperty] private NavItem _selectedItem;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageScroll))]
    private ObservableObject _currentPage;

    /// <summary>
    /// Pages scroll as a whole, except pages that fit the window height and scroll their own parts
    /// (<see cref="IFitsWindowHeight"/>, e.g. the presets list and editor).
    /// </summary>
    public ScrollBarVisibility PageScroll => CurrentPage is IFitsWindowHeight ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
    [ObservableProperty] private string _versionText = "";
    [ObservableProperty] private string _updateText = "";
    [ObservableProperty] private string _updateTip = "";

    /// <summary>Downloads and runs the new installer (or opens the download page in a portable copy).</summary>
    [RelayCommand]
    private Task DownloadUpdate() => AppHost.Get<WindowService>().StartUpdateAsync(Updates);

    private void OnUpdatesChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (UpdateTexts.Affects(e.PropertyName))
            RefreshVersionTexts();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        foreach (var item in Items)
            item.Refresh(_loc);
        RefreshVersionTexts();
        CurrentPage = CreatePage(SelectedItem); // rebuild so code-generated labels update too
    }

    private void RefreshVersionTexts()
    {
        VersionText = _loc.Format("about.version", UpdateService.CurrentVersion);
        UpdateText = UpdateTexts.Short(Updates, _loc);
        var changes = UpdateTexts.Changes(Updates, _loc);
        UpdateTip = UpdateTexts.Status(Updates, _loc)
                    ?? $"{_loc[$"update.level.{Updates.Level}"]}{Environment.NewLine}{(changes.Length > 0 ? changes + Environment.NewLine : "")}{_loc[UpdateTexts.HowKey]}";
    }

    public void Navigate(string key)
    {
        var item = Items.FirstOrDefault(i => i.Key == key);
        if (item is not null)
            SelectedItem = item;
    }

    // Ordinary pages are recreated on every visit. The batch-renaming session is shared across
    // navigation and main windows so a running conversion is never cancelled by leaving the page.
    // A navigation list that loses its
    // selection keeps the open page (Choices.Restore).
    partial void OnSelectedItemChanged(NavItem? oldValue, NavItem newValue)
    {
        if (!Choices.Restore(oldValue, newValue, v => SelectedItem = v))
            CurrentPage = CreatePage(newValue);
        OnPropertyChanged(nameof(SelectedPrimaryItem));
        OnPropertyChanged(nameof(IsGeneralSelected));
        OnPropertyChanged(nameof(IsAboutSelected));
    }

    // Old pages may hold event subscriptions on long-lived services. The batch rename
    // view model is a singleton session: disposing it would cancel an active conversion.
    partial void OnCurrentPageChanged(ObservableObject? oldValue, ObservableObject newValue)
    {
        if (oldValue is not BatchRenamePageViewModel)
            (oldValue as IDisposable)?.Dispose();
    }

    private ObservableObject CreatePage(NavItem item) =>
        item.PageType == typeof(BatchRenamePageViewModel)
            ? _services.GetRequiredService<BatchRenamePageViewModel>()
            : (ObservableObject)ActivatorUtilities.CreateInstance(_services, item.PageType);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Updates.PropertyChanged -= OnUpdatesChanged;
        _loc.LanguageChanged -= OnLanguageChanged;
        if (CurrentPage is not BatchRenamePageViewModel)
            (CurrentPage as IDisposable)?.Dispose();
    }
}
