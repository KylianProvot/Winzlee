using Filee.App.Services;

namespace Filee.App.Tests;

public sealed class WinzleeHealthCheckTests
{
    [Fact]
    public void Missing_dependencies_are_reported_without_creating_files()
    {
        var app = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var result = WinzleeHealthCheck.Scan(app, Path.Combine(app, "data"), true, false);
        Assert.False(result.DataFolderExists);
        Assert.False(result.SevenZipPresent);
        Assert.False(result.HatchZipPresent);
        Assert.False(result.StartupEnabled);
        Assert.False(Directory.Exists(app));
    }

    [Fact]
    public void Present_dependencies_are_detected_without_running_executables()
    {
        var root = Path.Combine(Path.GetTempPath(), "winzlee-health-" + Guid.NewGuid().ToString("N"));
        try
        {
            var app = Path.Combine(root, "app");
            var data = Path.Combine(root, "data");
            Directory.CreateDirectory(Path.Combine(app, "engines", "7zip"));
            Directory.CreateDirectory(Path.Combine(app, "engines", "hatchzip"));
            Directory.CreateDirectory(data);
            File.WriteAllText(Path.Combine(app, "engines", "7zip", "7z.exe"), "");
            File.WriteAllText(Path.Combine(app, "engines", "7zip", "7z.dll"), "");
            File.WriteAllText(Path.Combine(app, "engines", "hatchzip", "hatchzip.exe"), "");
            var result = WinzleeHealthCheck.Scan(app, data, true, true);
            Assert.True(result.DataFolderExists);
            Assert.True(result.SevenZipPresent);
            Assert.True(result.HatchZipPresent);
            Assert.True(result.StartupEnabled);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Non_windows_environment_does_not_claim_to_have_windows_tools()
    {
        var result = WinzleeHealthCheck.Scan(Path.GetTempPath(), Path.GetTempPath(), false, true);
        Assert.False(result.IsWindows);
        Assert.False(result.SevenZipPresent);
        Assert.False(result.HatchZipPresent);
        Assert.False(result.StartupEnabled);
    }

    [Fact]
    public void Missing_7zip_library_is_reported()
    {
        var root = Path.Combine(Path.GetTempPath(), "winzlee-health-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "engines", "7zip"));
            File.WriteAllText(Path.Combine(root, "engines", "7zip", "7z.exe"), "");
            Assert.False(WinzleeHealthCheck.Scan(root, root, true, false).SevenZipPresent);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
