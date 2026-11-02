using Filee.App.Services;
using Filee.App.ViewModels;
using Filee.Core.Formats;
using Filee.Core.Localization;
using Filee.Core.Presets;

namespace Filee.App.Tests;

public sealed class WinzleeArchiveChoiceTests
{
    [Theory]
    [InlineData("game.zip")]
    [InlineData("game.7z")]
    [InlineData("game.rar")]
    [InlineData("game.tar.gz")]
    public void Wheel_extraction_detects_archives(string name)
    {
        var p = new Preset { Id = "extract", TargetFormat = FormatRegistry.Folder };
        Assert.True(ArchiveChoicePolicy.IsExtraction(p, [name]));
        Assert.True(ArchiveChoicePolicy.ShouldPrompt(p, [name], browserDrop: false));
    }

    [Fact]
    public void Non_archive_or_browser_drop_does_not_open_archive_chooser()
    {
        var extract = new Preset { Id = "extract", TargetFormat = FormatRegistry.Folder };
        Assert.False(ArchiveChoicePolicy.ShouldPrompt(extract, ["photo.jpg"], browserDrop: false));
        Assert.False(ArchiveChoicePolicy.ShouldPrompt(extract, ["game.zip"], browserDrop: true));
    }

    [Theory]
    [InlineData("backup.zip", true)]
    [InlineData("backup.7z.001", true)]
    [InlineData("backup.rar", true)]
    [InlineData("backup.tar.gz", true)]
    [InlineData("backup.alz", false)]
    [InlineData("backup.egg", false)]
    public void Optimized_extraction_only_offered_for_hatchzip_containers(string name, bool expected) =>
        Assert.Equal(expected, ArchiveChoicePolicy.HatchZipCanRead(name));

    [Fact]
    public void Packaging_uses_own_copy_without_mutating_preset()
    {
        var original = new Preset { Id = "zip-all", TargetFormat = "zip" };
        original.Archive.CombineIntoOne = true;
        original.Archive.Level = ArchiveLevel.Fast;
        var copy = ArchiveChoicePolicy.ForCompression(original, "7z", ArchiveLevel.Maximum);
        Assert.Equal("zip", original.TargetFormat);
        Assert.Equal(ArchiveLevel.Fast, original.Archive.Level);
        Assert.Equal("7z", copy.TargetFormat);
        Assert.Equal(ArchiveLevel.Maximum, copy.Archive.Level);
        Assert.True(copy.PacksIntoOneArchive);
    }

    [Fact]
    public void Space_saving_requires_a_second_explicit_acknowledgement()
    {
        var p = new Preset { Id = "extract", TargetFormat = FormatRegistry.Folder };
        var vm = new ArchiveChoiceViewModel(["game.zip"], p, hatchAvailable: true, new StubLoc());
        Assert.True(vm.CanStart); // Classic by default.
        Assert.True(vm.ClassicSelected);
        vm.OptimizedSelected = true;
        Assert.False(vm.CanStart);
        vm.Acknowledged = true;
        Assert.True(vm.CanStart);
        vm.ClassicSelected = true;
        Assert.False(vm.OptimizedSelected);
        Assert.False(vm.Acknowledged); // A later destructive choice must reconfirm.
    }

    [Fact]
    public void Hatchzip_missing_keeps_classic_mode_available()
    {
        var p = new Preset { Id = "extract", TargetFormat = FormatRegistry.Folder };
        var vm = new ArchiveChoiceViewModel(["game.zip"], p, hatchAvailable: false, new StubLoc());
        Assert.True(vm.CanStart);
        Assert.False(vm.HatchAvailable);
        vm.OptimizedSelected = true;
        vm.Acknowledged = true;
        Assert.False(vm.CanStart);
    }

    [Theory]
    [InlineData("photos.zip", "photos")]
    [InlineData("photos.7z.001", "photos")]
    [InlineData("photos.tar.gz", "photos")]
    [InlineData("photos.tgz", "photos")]
    public void Custom_hatchzip_destinations_use_a_named_subfolder(string archive, string expected)
    {
        Assert.Equal(Path.Combine("another-place", expected),
            ArchiveChoicePolicy.HatchOutputDirectory(archive, "another-place"));
    }

    [Fact]
    public void Archive_dialog_starts_with_source_folder()
    {
        var archive = Path.Combine(Path.GetTempPath(), "test-input.zip");
        var vm = new ArchiveChoiceViewModel([archive],
            new Preset { Id = "extract", TargetFormat = FormatRegistry.Folder },
            hatchAvailable: true, new StubLoc());
        Assert.Equal(Path.GetDirectoryName(Path.GetFullPath(archive)), vm.Destination);
        Assert.True(vm.ClassicSelected);
    }

    [Fact]
    public void Compression_chooser_uses_source_folder_and_exposes_destination()
    {
        var source = Path.Combine(Path.GetTempPath(), "pictures");
        var vm = new ArchiveChoiceViewModel([source],
            new Preset { Id = "pack", TargetFormat = "zip" }, hatchAvailable: true, new StubLoc());
        Assert.True(vm.IsCompression);
        Assert.Equal(Path.GetDirectoryName(Path.GetFullPath(source)), vm.Destination);
        Assert.Equal("winzlee.archive.compress_destination", vm.DestinationLabel);
        Assert.Equal("winzlee.archive.help.zip", vm.FormatHelp);
    }

    [Fact]
    public void Hatchzip_toast_uses_an_indeterminate_progress_until_extraction_finishes()
    {
        var dismissed = false;
        using var toast = new HatchZipToastViewModel(["game.zip"], new StubLoc(), _ => dismissed = true);
        Assert.True(toast.IsRunning);
        Assert.True(toast.IsIndeterminate);
        toast.Report("extract", "game.zip");
        Assert.Contains("winzlee.archive.extracting_file", toast.Status);
        toast.Complete();
        Assert.False(toast.IsRunning);
        Assert.False(toast.IsIndeterminate);
        Assert.Equal(100, toast.Progress);
        toast.Report("check", "game.zip");
        Assert.Equal("winzlee.archive.finished", toast.Status);
        toast.DismissCommand.Execute(null);
        Assert.True(dismissed);
    }

    [Fact]
    public void Hatchzip_cancel_keeps_recovery_instructions_visible()
    {
        using var toast = new HatchZipToastViewModel(["game.zip"], new StubLoc(), _ => { });
        toast.DismissCommand.Execute(null); // Closing a running extraction requests cancellation.
        Assert.True(toast.CancellationToken.IsCancellationRequested);
        Assert.True(toast.IsRunning);
        toast.Interrupted();
        Assert.True(toast.HasErrors);
        Assert.False(toast.IsRunning);
        Assert.Equal("winzlee.archive.interrupted", toast.Status);
        Assert.NotNull(toast.ErrorDetails);
    }

    private sealed class StubLoc : ILocalizer
    {
        public string Language => "en";
        public string this[string key] => key;
        public string Format(string key, params object[] args) => key + " " + string.Join(", ", args);
        public event EventHandler? LanguageChanged { add { } remove { } }
    }
}
