// WINZLEE_DATA_DIR moves the data folder, so a development or test copy never reads or writes the user's own Filee.

using Filee.Core.Settings;

namespace Filee.Core.Tests;

public class DataDirectoryTests
{
    [Fact]
    public void The_variable_moves_the_default_data_folder()
    {
        using var dir = new TempDir();
        var before = Environment.GetEnvironmentVariable(UserDataStore.DirectoryVariable);
        try
        {
            Environment.SetEnvironmentVariable(UserDataStore.DirectoryVariable, dir.Path + Path.DirectorySeparatorChar);
            Assert.Equal(Path.GetFullPath(dir.Path), UserDataStore.OverrideDirectory);
            Assert.Equal(Path.GetFullPath(dir.Path), UserDataStore.DefaultDirectory);

            Environment.SetEnvironmentVariable(UserDataStore.DirectoryVariable, null);
            Assert.Null(UserDataStore.OverrideDirectory);
            Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Winzlee"),
                UserDataStore.DefaultDirectory);
        }
        finally
        {
            Environment.SetEnvironmentVariable(UserDataStore.DirectoryVariable, before);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_variable_is_no_override(string? value) => Assert.Null(UserDataStore.DirectoryFrom(value));

    [Fact]
    public void A_relative_folder_is_made_absolute() =>
        Assert.Equal(Path.Combine(Environment.CurrentDirectory, "filee-test"), UserDataStore.DirectoryFrom("filee-test"));
    [Fact]
    public void Winzlee_adds_ctrl_right_click_without_reinstalling_a_shortcut_deleted_by_user()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var directory = Directory.CreateTempSubdirectory("winzlee-click-migration-").FullName;
        try
        {
            var store = new UserDataStore(directory);
            store.Load();
            Assert.True(store.Settings.WinzleeCtrlRightMigrationDone);
            Assert.Single(store.Settings.Triggers, g =>
                g.Kind == TriggerKind.Click
                && g.Modifiers == ModifierKeys.Ctrl && g.Button == TriggerMouseButton.Right);

            store.Settings.Triggers.RemoveAll(g => g.Kind == TriggerKind.Click);
            store.SaveSettings();

            var reload = new UserDataStore(directory);
            reload.Load();
            Assert.True(reload.Settings.WinzleeCtrlRightMigrationDone);
            Assert.DoesNotContain(reload.Settings.Triggers, g => g.Kind == TriggerKind.Click);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

}
