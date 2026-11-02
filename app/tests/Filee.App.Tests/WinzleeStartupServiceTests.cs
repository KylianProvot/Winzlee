using Filee.App.Services;

namespace Filee.App.Tests;

public sealed class WinzleeStartupServiceTests
{
    [Fact]
    public void Auto_start_command_quotes_spaces_and_uses_background()
    {
        if (!OperatingSystem.IsWindows()) return;
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "My App", "Winzlee.exe"));
        Assert.Equal("\"" + path + "\" --background", WinzleeStartupService.CommandFor(path));
    }

    [Theory]
    [InlineData("Filee.exe")]
    [InlineData("dotnet.exe")]
    [InlineData("")]
    public void Auto_start_refuses_other_executables(string executable)
    {
        Assert.Throws<ArgumentException>(() => WinzleeStartupService.CommandFor(executable));
    }

    [Fact]
    public void Auto_start_has_an_entry_distinct_from_filee()
    {
        Assert.Equal("Winzlee", WinzleeStartupService.EntryName);
        Assert.EndsWith(@"\Run", WinzleeStartupService.RunKey, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_executable_is_not_registered()
    {
        Assert.False(WinzleeStartupService.IsEnabled(null));
    }
}
