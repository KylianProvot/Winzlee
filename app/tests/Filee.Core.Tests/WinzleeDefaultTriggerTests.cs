using Filee.Core.Settings;

namespace Filee.Core.Tests;

public sealed class WinzleeDefaultTriggerTests
{
    [Fact]
    public void Windows_default_shortcut_is_first_enabled_and_explorer_only()
    {
        var defaults = TriggerGesture.Defaults();
        if (!OperatingSystem.IsWindows()) return;

        var shortcut = Assert.Single(defaults.Where(x => x.Id == "winzlee-ctrl-right"));
        Assert.Same(shortcut, defaults[0]);
        Assert.True(shortcut.Enabled);
        Assert.Equal(TriggerKind.Click, shortcut.Kind);
        Assert.Equal(ModifierKeys.Ctrl, shortcut.Modifiers);
        Assert.Equal(TriggerMouseButton.Right, shortcut.Button);
        Assert.Equal(TriggerScope.FileManager, shortcut.Scope);
        Assert.All(defaults.Skip(1), gesture => Assert.False(gesture.Enabled));
    }

    [Fact]
    public void Fresh_store_keeps_enabled_shortcut_on_reload()
    {
        if (!OperatingSystem.IsWindows()) return;
        var directory = Path.Combine(Path.GetTempPath(), "winzlee-trigger-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new UserDataStore(directory);
            store.Load();
            Assert.Equal("winzlee-ctrl-right", store.Settings.Triggers[0].Id);
            Assert.True(store.Settings.Triggers[0].Enabled);
            store.SaveSettings();

            var reloaded = new UserDataStore(directory);
            reloaded.Load();
            Assert.Equal("winzlee-ctrl-right", reloaded.Settings.Triggers[0].Id);
            Assert.True(reloaded.Settings.Triggers[0].Enabled);

            reloaded.Settings.Triggers[0].Enabled = false;
            reloaded.SaveSettings();
            var custom = new UserDataStore(directory);
            custom.Load();
            Assert.False(custom.Settings.Triggers[0].Enabled);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
