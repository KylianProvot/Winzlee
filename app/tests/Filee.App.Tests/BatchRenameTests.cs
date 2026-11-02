using Filee.App.Services;
using Filee.Core.Formats;

namespace Filee.App.Tests;

public sealed class BatchRenameTests
{
    [Fact]
    public void Advanced_options_can_replace_original_names_add_prefix_and_change_case()
    {
        using var files = new TempFiles();
        var input = files.Add("IMG_0412.JPEG");
        var row = BatchRenamePlan.Build([input],
            new BatchRenameOptions("ignored", 1, "-", null,
                UseOriginalNames: true, AddNumbers: false, Find: "IMG_", Replace: "Photo_",
                Prefix: "Travel_", Suffix: "_final", LetterCase: "lower"))[0];
        Assert.True(row.IsValid);
        Assert.Equal("travel_photo_0412_final.JPEG", row.NewName);
    }

    private sealed class TempFiles : IDisposable
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(), "winzlee-batch-" + Guid.NewGuid().ToString("N"));

        public TempFiles() => Directory.CreateDirectory(Folder);

        public string Add(string name)
        {
            var path = Path.Combine(Folder, name);
            File.WriteAllText(path, "sample");
            return path;
        }

        public void Dispose() => Directory.Delete(Folder, recursive: true);
    }

    [Fact]
    public void Mixed_extensions_are_renamed_in_one_numbered_batch_without_conversion()
    {
        using var files = new TempFiles();
        var a = files.Add("IMG_01.png");
        var b = files.Add("IMG_02.jpeg");
        var c = files.Add("notes.customext");
        var rows = BatchRenamePlan.Build([a, b, c], new BatchRenameOptions("Photo", 1, " ", null));

        Assert.Equal(new[] { "Photo 1.png", "Photo 2.jpeg", "Photo 3.customext" }, rows.Select(x => x.NewName));
        Assert.All(rows, x => Assert.True(x.IsValid));
        Assert.All(rows, x => Assert.True(File.Exists(x.Source))); // Preview never changes disk.
    }

    [Fact]
    public void Compatible_mixed_images_can_target_one_format()
    {
        using var files = new TempFiles();
        var rows = BatchRenamePlan.Build(
            [files.Add("first.png"), files.Add("second.jpeg")],
            new BatchRenameOptions("Holiday", 5, "_", "pdf"),
            (from, to) => from is "png" or "jpg" && to == "pdf");

        Assert.Equal(new[] { "Holiday_5.pdf", "Holiday_6.pdf" }, rows.Select(x => x.NewName));
        Assert.All(rows, x => Assert.Null(x.Error));
    }

    [Fact]
    public void Unsupported_conversions_are_rejected_but_renaming_any_extension_is_allowed()
    {
        using var files = new TempFiles();
        var unknown = files.Add("scan.mycustom");
        Assert.True(BatchRenamePlan.Build([unknown], new BatchRenameOptions("A", 1, "-", null))[0].IsValid);
        Assert.Equal("unsupported", BatchRenamePlan.Build([unknown], new BatchRenameOptions("A", 1, "-", "pdf"))[0].Error);
    }

    [Fact]
    public void Existing_output_never_gets_overwritten()
    {
        using var files = new TempFiles();
        var a = files.Add("original.jpg");
        files.Add("Photo 1.jpg");
        var planned = BatchRenamePlan.Build([a], new BatchRenameOptions("Photo", 1, " ", null));
        Assert.Equal("exists", planned[0].Error);
    }

    [Fact]
    public void Compound_extensions_are_preserved_during_renaming()
    {
        using var files = new TempFiles();
        var archive = files.Add("data.tar.gz");
        var item = BatchRenamePlan.Build([archive],
            new BatchRenameOptions("Backup", 1, " ", null, UseOriginalNames: false))[0];
        Assert.Equal("Backup 1.tar.gz", item.NewName);
        var withOriginal = BatchRenamePlan.Build([archive],
            new BatchRenameOptions("unused", 1, "-", null, UseOriginalNames: true, AddNumbers: false))[0];
        Assert.Equal("data.tar.gz", withOriginal.NewName);
    }

    [Fact]
    public void Invalid_windows_names_and_same_path_conversion_are_blocked()
    {
        Assert.False(BatchRenamePlan.ValidStem("CON"));
        Assert.False(BatchRenamePlan.ValidStem("Photo?"));
        Assert.False(BatchRenamePlan.ValidStem("Photo."));
        using var files = new TempFiles();
        var a = files.Add("Photo 1.png");
        Assert.Equal("same-path", BatchRenamePlan.Build([a],
            new BatchRenameOptions("Photo", 1, " ", "png"))[0].Error);
    }

    [Fact]
    public void Case_only_rename_is_not_discarded_and_can_be_reversed()
    {
        using var files = new TempFiles();
        var original = files.Add("photo.png");
        var preview = BatchRenamePlan.Build([original],
            new BatchRenameOptions("PHOTO", 1, "-", null, AddNumbers: false))[0];
        Assert.True(preview.IsValid);
        Assert.True(preview.ChangesPath);
        BatchRenamePlan.MoveFileExact(original, preview.Destination);
        Assert.True(File.Exists(preview.Destination));
        BatchRenamePlan.MoveFileExact(preview.Destination, original);
        Assert.True(File.Exists(original));
    }

    [Fact]
    public void Missing_custom_folder_is_refused_before_touching_files()
    {
        using var files = new TempFiles();
        var a = files.Add("data.txt");
        var missing = Path.Combine(files.Folder, "not-existing");
        var rows = BatchRenamePlan.Build([a], new BatchRenameOptions("A", 1, "-", null, missing));
        Assert.Equal("folder", rows[0].Error);
        Assert.True(File.Exists(a));
    }

    [Fact]
    public void Identical_inputs_are_deduplicated_without_losing_sequence()
    {
        using var files = new TempFiles();
        var a = files.Add("one.png");
        var b = files.Add("two.png");
        var result = BatchRenamePlan.Build([a, a, b], new BatchRenameOptions("Picture", 1, "-", null));
        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "Picture-1.png", "Picture-2.png" }, result.Select(x => x.NewName));
    }
}
