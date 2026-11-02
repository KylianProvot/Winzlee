// Installs, updates and uninstalls against a registry in memory, a fake Windows and scratch folders: nothing here
// touches HKLM, HKCU, PATH, the Start menu or a real Filee.

extern alias app;
using Filee.Setup.Installation;
using Filee.Setup.Platform;
using CommandLine = app::Filee.App.Services.CommandLine;

namespace Filee.Setup.Tests;

public class InstallerTests
{
    private const string Key = SetupNames.UninstallKey;

    [Fact]
    public void Winzlee_and_filee_have_independent_downloaded_engine_folders()
    {
        using var bench = new SetupBench();
        Assert.Equal(Path.Combine(bench.Paths.LocalAppData, "Winzlee", "engines"), bench.Paths.EnginesFolder);
        Assert.Equal(Path.Combine(bench.Paths.LocalAppData, "Filee", "engines"), bench.Paths.LegacyEnginesFolder);
        Assert.NotEqual(bench.Paths.EnginesFolder, bench.Paths.LegacyEnginesFolder);
    }

    [Fact]
    public void A_fresh_install_does_not_replace_files_in_an_unrecognized_folder()
    {
        using var bench = new SetupBench();
        Directory.CreateDirectory(bench.InstallFolder);
        var personalFile = Path.Combine(bench.InstallFolder, "notes.txt");
        File.WriteAllText(personalFile, "Keep this file");
        var result = bench.Install();

        Assert.False(result.Succeeded);
        Assert.Contains("not empty", result.Error?.Message);
        Assert.Equal("Keep this file", File.ReadAllText(personalFile));
        Assert.False(File.Exists(Path.Combine(bench.InstallFolder, SetupNames.AppExe)));
        Assert.False(bench.Registry.KeyExists(Hive.LocalMachine, SetupNames.UninstallKey));
    }

    [Fact]
    public void A_fresh_install_puts_everything_in_place()
    {
        using var bench = new SetupBench();
        var result = bench.Install();
        Assert.True(result.Succeeded, result.Error?.ToString());
        var folder = bench.InstallFolder;

        foreach (var file in new[] { "Winzlee.exe", "Filee.dll", @"cli\filee.cmd", @"engines\rhwp\rhwp.exe", "uninstall.exe", "uninstall.json" })
            Assert.True(File.Exists(Path.Combine(folder, file)), file);
        Assert.Empty(Directory.EnumerateDirectories(folder, FileSwap.WorkFolderPrefix + "*"));
        var record = InstallRecord.Read(folder)!;
        Assert.Equal("2.0.0", record.Version);
        Assert.Contains("uninstall.exe", record.Files);
        Assert.Contains(@"engines\rhwp\rhwp.exe", record.Files);

        // Apps & Features: Inno Setup's key, with everything Windows shows.
        var registry = bench.Registry;
        string? Value(string name) => registry.Get(Hive.LocalMachine, Key, name)?.Text;
        Assert.Equal("Winzlee", Value("DisplayName"));
        Assert.Equal("2.0.0", Value("DisplayVersion"));
        Assert.Equal("Kylian Provot", Value("Publisher"));
        Assert.Equal(Path.Combine(folder, "Winzlee.exe"), Value("DisplayIcon"));
        Assert.Equal(folder + "\\", Value("InstallLocation"));
        Assert.Equal($"\"{folder}\\uninstall.exe\"", Value("UninstallString"));
        Assert.Equal($"\"{folder}\\uninstall.exe\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART", Value("QuietUninstallString"));
        Assert.Equal(1, registry.Get(Hive.LocalMachine, Key, "NoModify")?.Value);
        Assert.True((int)registry.Get(Hive.LocalMachine, Key, "EstimatedSize")!.Value > 0);
        Assert.Equal(2, registry.Get(Hive.LocalMachine, Key, "VersionMajor")?.Value);
        Assert.Equal(@"contextmenu,contextmenu\top,startup,cli", Value(SetupNames.TasksValue));

        // Start menu, PATH (REG_EXPAND_SZ), no desktop shortcut by default.
        Assert.True(File.Exists(Path.Combine(bench.Paths.CommonStartMenu, "Winzlee.lnk")));
        Assert.False(File.Exists(Path.Combine(bench.Paths.CommonDesktop, "Winzlee.lnk")));
        var path = registry.Get(Hive.LocalMachine, SetupNames.MachineEnvironmentKey, "Path")!;
        Assert.True(path.Expandable);
        Assert.True(PathVariable.Contains(path.Text, Path.Combine(folder, "cli")));
        Assert.Contains("broadcast", bench.System.Calls);

        // Filee starts with the choices; it applies them for the signed-in user.
        Assert.Equal(Path.Combine(folder, "Winzlee.exe"), result.Launch!.Executable);
        Assert.Equal("--install-engines=pandoc --start-with-windows=on --context-menu=on", result.Launch.Arguments);
    }

    [Fact]
    public void The_windows_11_menu_is_registered_when_ticked()
    {
        using var bench = new SetupBench();
        bench.System.ExplorerMenuSupported = true;
        Assert.True(bench.Install().Succeeded);
        Assert.Contains($"register-menu {bench.InstallFolder} True", bench.System.Calls);
    }

    [Fact]
    public void A_fresh_install_without_the_top_menu_removes_an_old_registration()
    {
        using var bench = new SetupBench();
        bench.System.ExplorerMenuSupported = true;
        bench.System.MenuRegistered = true;
        Assert.True(bench.Install(bench.Choices(SetupTasks.Defaults with { ModernMenu = false })).Succeeded);
        Assert.Contains("unregister-menu", bench.System.Calls);
        Assert.DoesNotContain(bench.System.Calls, c => c.StartsWith("register-menu", StringComparison.Ordinal));
    }

    [Fact]
    public void A_failed_menu_registration_is_a_warning_not_a_failure()
    {
        using var bench = new SetupBench();
        bench.System.ExplorerMenuSupported = true;
        bench.System.RegisterError = "0x80073CF3 the package failed update";
        var result = bench.Install();
        Assert.True(result.Succeeded);
        Assert.Equal("0x80073CF3 the package failed update", Assert.IsType<ExplorerMenuWarning>(Assert.Single(result.Warnings)).Error);
    }

    [Fact]
    public void An_update_leaves_a_current_menu_registration_alone()
    {
        // Registering the same package again fails while Explorer has the menu loaded (0x80073D02).
        using var bench = new SetupBench();
        bench.System.ExplorerMenuSupported = true;
        Assert.True(bench.Install().Succeeded);
        bench.System.Calls.Clear();
        bench.System.MenuCurrent = true;
        bench.System.RegisterError = "0x80073D02 the package is in use";
        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        var result = bench.Install(bench.Choices(previous.CurrentTasks(bench.Registry, bench.System, bench.Paths)) with { Engines = [] }, previous);
        Assert.True(result.Succeeded, result.Error?.ToString());
        Assert.Empty(result.Warnings);
        Assert.DoesNotContain(bench.System.Calls, c => c.Contains("register-menu", StringComparison.Ordinal));
    }

    [Fact]
    public void An_existing_upstream_filee_installation_is_not_claimed_by_winzlee()
    {
        using var bench = new SetupBench();
        var fileeFolder = bench.Temp.Sub("upstream-filee");
        Directory.CreateDirectory(fileeFolder);
        File.WriteAllText(Path.Combine(fileeFolder, "Filee.exe"), "Original Filee");
        const string upstreamKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{C8686E06-8327-40A2-B0F7-3DEDD61D9248}_is1";
        bench.Registry.SetString(Hive.LocalMachine, upstreamKey, "InstallLocation", fileeFolder);
        bench.Registry.SetString(Hive.LocalMachine, upstreamKey, "DisplayName", "Filee");
        Assert.Null(PreviousInstall.Find(bench.Registry, InstallScope.Machine));

        var result = bench.Install();
        Assert.True(result.Succeeded, result.Error?.ToString());
        Assert.Equal("Original Filee", File.ReadAllText(Path.Combine(fileeFolder, "Filee.exe")));
        Assert.Equal("Filee", bench.Registry.Get(Hive.LocalMachine, upstreamKey, "DisplayName")?.Text);
        Assert.Equal("Winzlee", bench.Registry.Get(Hive.LocalMachine, Key, "DisplayName")?.Text);
    }

    [Fact]
    public void An_update_with_a_missing_manifest_cannot_overwrite_existing_files()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install().Succeeded);
        var executable = Path.Combine(bench.InstallFolder, SetupNames.AppExe);
        var original = File.ReadAllText(executable);
        File.Delete(Path.Combine(bench.InstallFolder, SetupNames.InstallRecordFile));

        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        Assert.Null(previous.Record);
        File.WriteAllText(Path.Combine(bench.AppFolder, SetupNames.AppExe), "replacement");
        bench.MakeSetup("2.1.0");

        var result = bench.Install(previous: previous);
        Assert.False(result.Succeeded);
        Assert.Contains("uninstall.json", result.Error?.Message);
        Assert.Equal(original, File.ReadAllText(executable));
        Assert.Equal("2.0.0", bench.Registry.Get(Hive.LocalMachine, Key, "DisplayVersion")?.Text);
    }

    [Fact]
    public void Updating_our_own_install_replaces_exactly_its_files()
    {
        using var bench = new SetupBench(version: "2.0.0", app: (temp, _) => temp.Write(@"app\Removed.dll", "only in 2.0.0"));
        Assert.True(bench.Install().Succeeded);
        var folder = bench.InstallFolder;
        // Something of the user's in the folder stays.
        File.WriteAllText(Path.Combine(folder, "notes.txt"), "mine");
        File.Delete(Path.Combine(bench.AppFolder, "Removed.dll"));
        File.WriteAllText(Path.Combine(bench.AppFolder, "Winzlee.exe"), "Winzlee 2.1");
        bench.MakeSetup("2.1.0");

        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        Assert.False(previous.ByInno);
        Assert.NotNull(previous.Record);
        var result = bench.Install(bench.Choices(previous.CurrentTasks(bench.Registry, bench.System, bench.Paths)) with { Engines = [] }, previous);
        Assert.True(result.Succeeded, result.Error?.ToString());
        Assert.False(File.Exists(Path.Combine(folder, "Removed.dll")));
        Assert.True(File.Exists(Path.Combine(folder, "notes.txt")));
        Assert.Equal("Winzlee 2.1", File.ReadAllText(Path.Combine(folder, "Winzlee.exe")));
        Assert.Equal("2.1.0", InstallRecord.Read(folder)!.Version);
        Assert.Equal("2.1.0", bench.Registry.Get(Hive.LocalMachine, Key, "DisplayVersion")?.Text);
    }

    [Fact]
    public void Unticking_on_an_update_removes_the_desktop_icon_and_the_path_entry()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install(bench.Choices(SetupTasks.Defaults with { DesktopIcon = true })).Succeeded);
        var desktop = Path.Combine(bench.Paths.CommonDesktop, "Winzlee.lnk");
        Assert.True(File.Exists(desktop));
        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        var tasks = previous.CurrentTasks(bench.Registry, bench.System, bench.Paths) with { DesktopIcon = false, Cli = false };
        Assert.True(bench.Install(bench.Choices(tasks), previous).Succeeded);
        Assert.False(File.Exists(desktop));
        Assert.False(PathVariable.IsOnPath(bench.Registry, InstallScope.Machine, Path.Combine(bench.InstallFolder, "cli")));
    }

    [Fact]
    public void A_failure_halfway_puts_the_previous_version_back()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install().Succeeded);
        var folder = bench.InstallFolder;
        var before = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .ToDictionary(f => f, File.ReadAllText);
        var entryBefore = bench.Registry.ValueNames(Hive.LocalMachine, Key)
            .ToDictionary(n => n, n => bench.Registry.Get(Hive.LocalMachine, Key, n)!.Value);
        File.WriteAllText(Path.Combine(bench.AppFolder, "Winzlee.exe"), "Winzlee 3");
        bench.MakeSetup("3.0.0");
        File.Delete(Path.Combine(bench.Paths.CommonStartMenu, "Winzlee.lnk"));

        // The Start menu shortcut can't be written: everything after the file swap fails.
        bench.System.OnCreateShortcut = _ => throw new IOException("Access denied (test)");
        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        var result = bench.Install(previous: previous);
        Assert.False(result.Succeeded);
        Assert.True(result.RolledBack);
        Assert.Null(result.Launch);

        var after = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).ToDictionary(f => f, File.ReadAllText);
        Assert.Equal(before, after);
        Assert.Equal("2.0.0", bench.Registry.Get(Hive.LocalMachine, Key, "DisplayVersion")?.Text);
        Assert.Equal(entryBefore.Keys.Order(), bench.Registry.ValueNames(Hive.LocalMachine, Key).Order());
    }

    [Fact]
    public void A_damaged_payload_changes_nothing()
    {
        using var bench = new SetupBench();
        long offset;
        using (var payload = Filee.Setup.Payload.SetupPayload.Open(bench.SetupExe)!)
            offset = payload.Offset;
        var bytes = File.ReadAllBytes(bench.SetupExe);
        bytes[offset + 3] ^= 0x55;
        File.WriteAllBytes(bench.SetupExe, bytes);

        var result = bench.Install();
        Assert.False(result.Succeeded);
        Assert.IsType<InvalidDataException>(result.Error);
        Assert.False(result.RolledBack);
        Assert.False(Directory.Exists(bench.InstallFolder));
        Assert.False(bench.Registry.KeyExists(Hive.LocalMachine, Key));
        Assert.Empty(bench.System.Calls);
    }

    [Fact]
    public void No_icons_and_portable()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install(bench.Choices() with { NoIcons = true }).Succeeded);
        Assert.False(File.Exists(Path.Combine(bench.Paths.CommonStartMenu, "Winzlee.lnk")));

        using var portable = new SetupBench();
        var folder = portable.Temp.Sub("Portable Filee");
        var result = portable.Install(portable.Choices() with { Folder = folder, Portable = true }, previous: null);
        Assert.True(result.Succeeded);
        Assert.Null(result.Launch);
        Assert.True(File.Exists(Path.Combine(folder, "Winzlee.exe")));
        // No uninstaller: the app counts itself as portable (UpdateService.IsInstalled).
        Assert.False(File.Exists(Path.Combine(folder, "uninstall.exe")));
        Assert.False(portable.Registry.KeyExists(Hive.LocalMachine, Key));
        Assert.Empty(portable.System.Calls);
    }

    [Fact]
    public void A_per_user_install_stays_in_the_users_part()
    {
        using var bench = new SetupBench();
        var folder = bench.Paths.DefaultInstallFolder(InstallScope.User);
        var result = bench.Install(bench.Choices() with { Folder = folder, Scope = InstallScope.User }, previous: null);
        Assert.True(result.Succeeded);
        Assert.True(bench.Registry.KeyExists(Hive.CurrentUser, Key));
        Assert.False(bench.Registry.KeyExists(Hive.LocalMachine, Key));
        Assert.True(PathVariable.IsOnPath(bench.Registry, InstallScope.User, Path.Combine(folder, "cli")));
        Assert.True(File.Exists(Path.Combine(bench.Paths.UserStartMenu, "Winzlee.lnk")));
        Assert.Equal(InstallScope.User, InstallRecord.Read(folder)!.Scope);
        Assert.NotNull(PreviousInstall.Find(bench.Registry, InstallScope.User));
    }

    [Fact]
    public void Existing_filee_velopack_install_is_left_untouched()
    {
        using var bench = new SetupBench();
        var root = VelopackInstall.Root(bench.Paths);
        bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), "Update.exe"));
        bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), @"current\Winzlee.exe"));
        bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), @"packages\Filee-1.1.0-full.nupkg"));
        var engine = bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), @"engines\pandoc\.filee-component"));
        bench.Registry.SetString(Hive.CurrentUser, SetupNames.VelopackUninstallKey, "DisplayName", "Filee");
        Assert.True(VelopackInstall.Exists(bench.Paths));

        Assert.True(bench.Install().Succeeded);
        Assert.True(File.Exists(Path.Combine(root, "Update.exe")));
        Assert.True(Directory.Exists(Path.Combine(root, "current")));
        Assert.True(Directory.Exists(Path.Combine(root, "packages")));
        Assert.True(File.Exists(engine));
        Assert.True(bench.Registry.KeyExists(Hive.CurrentUser, SetupNames.VelopackUninstallKey));
    }

    [Theory]
    [InlineData("ko")]
    [InlineData("pt-PT")]
    [InlineData("pt-BR")]
    public void What_setup_hands_to_filee_is_what_filee_reads(string language)
    {
        var fresh = new InstallChoices(@"C:\Program Files\Filee", InstallScope.Machine, SetupTasks.Defaults with { Startup = false },
            ["pandoc", "ffmpeg"], language, LanguagePicked: true);
        var options = CommandLine.Parse(Installer.LaunchArguments(fresh, update: false).Split(' '));
        Assert.Equal(["pandoc", "ffmpeg"], options.InstallEngines);
        Assert.False(options.StartWithWindows);
        Assert.True(options.ContextMenu);
        Assert.Equal(language, options.Language);

        // Setup's language only goes to Filee when the user picked it; else Filee keeps following Windows.
        Assert.Null(CommandLine.Parse(Installer.LaunchArguments(fresh with { LanguagePicked = false }, update: false).Split(' ')).Language);
        // Nothing picked is an answer too: Filee then doesn't ask on first run.
        Assert.Empty(CommandLine.Parse(Installer.LaunchArguments(fresh with { Engines = [] }, update: false).Split(' ')).InstallEngines!);
        var updated = CommandLine.Parse(Installer.LaunchArguments(fresh, update: true).Split(' '));
        Assert.True(updated.Background);
        Assert.True(updated.WhatsNew);
        var shown = CommandLine.Parse(Installer.LaunchArguments(fresh, update: true, shown: true, updatedFrom: "1.12.0").Split(' '));
        Assert.False(shown.Background);
        Assert.True(shown.WhatsNew);
        Assert.Equal("1.12.0", shown.UpdatedFrom);
        // A version that isn't plain digits and dots stays off the command line.
        Assert.Equal("--background --whats-new", Installer.LaunchArguments(fresh, update: true, updatedFrom: "1.0 \"--quit"));
        Assert.False(CommandLine.Parse(Installer.LaunchArguments(fresh, update: false).Split(' ')).WhatsNew);
    }
}
