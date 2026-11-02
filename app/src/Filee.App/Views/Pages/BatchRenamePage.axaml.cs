using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Filee.App.Services;
using Filee.App.ViewModels.Pages;

namespace Filee.App.Views.Pages;

public partial class BatchRenamePage : UserControl
{
    public BatchRenamePage()
    {
        InitializeComponent();
        RenameDrop.PointerPressed += OnDropClicked;
        // A closed ComboBox must not steal mouse-wheel input intended to scroll this page.
        // While its dropdown is open, keep the usual wheel behavior inside the choices.
        // Capture wheel events at the page: controls in a collapsed Expander have not been
        // created yet, so looking up individual ComboBoxes here would fail at startup.
        this.AddHandler(InputElement.PointerWheelChangedEvent, OnClosedComboWheel,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        DragDrop.AddDragEnterHandler(RenameDrop, (_, e) =>
        {
            var offered = DraggedFiles.Offered(e.DataTransfer);
            e.DragEffects = offered ? DragDropEffects.Copy : DragDropEffects.None;
            if (offered) RenameDrop.Classes.Add("over");
            else RenameDrop.Classes.Remove("over");
        });
        DragDrop.AddDragOverHandler(RenameDrop, (_, e) =>
            e.DragEffects = DraggedFiles.Offered(e.DataTransfer) ? DragDropEffects.Copy : DragDropEffects.None);
        DragDrop.AddDragLeaveHandler(RenameDrop, (_, _) => RenameDrop.Classes.Remove("over"));
        DragDrop.AddDropHandler(RenameDrop, (_, e) =>
        {
            RenameDrop.Classes.Remove("over");
            if (DataContext is BatchRenamePageViewModel vm)
                vm.AddFiles(DraggedFiles.Local(e.DataTransfer));
            e.DragEffects = DragDropEffects.Copy;
        });
    }

    private void OnClosedComboWheel(object? sender, PointerWheelEventArgs e)
    {
        var combo = e.Source as ComboBox
            ?? (e.Source as Control)?.GetVisualAncestors().OfType<ComboBox>().FirstOrDefault();
        if (combo is null || combo.IsDropDownOpen || e.Handled)
            return;
        e.Handled = true; // Prevent the closed ComboBox from changing its value.
        if (TopLevel.GetTopLevel(this) is MainWindow window &&
            window.FindControl<ScrollViewer>("PageScroller") is { } page)
        {
            var max = Math.Max(0, page.Extent.Height - page.Viewport.Height);
            page.Offset = new Vector(page.Offset.X,
                Math.Clamp(page.Offset.Y - e.Delta.Y * 64, 0, max));
        }
    }

    private void OnRemoveFile(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string source } && DataContext is BatchRenamePageViewModel vm)
            vm.RemoveFile(source);
    }

    private async void OnDropClicked(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(RenameDrop).Properties.IsLeftButtonPressed)
            await PickFilesAsync();
    }

    private async void OnPickFiles(object? sender, RoutedEventArgs e) => await PickFilesAsync();

    private async Task PickFilesAsync()
    {
        if (DataContext is not BatchRenamePageViewModel vm || TopLevel.GetTopLevel(this) is not { } top)
            return;
        var selected = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true
        });
        vm.AddFiles(selected.Select(f => f.TryGetLocalPath()).OfType<string>());
    }

    private async void OnPickDestination(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BatchRenamePageViewModel vm || TopLevel.GetTopLevel(this) is not { } top)
            return;
        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });
        if (folders.FirstOrDefault()?.TryGetLocalPath() is { } path)
            vm.DestinationFolder = path;
    }
}
