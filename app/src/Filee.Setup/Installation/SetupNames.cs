// Winzlee has a distinct Windows installation identity; historical Filee keys must not be reused.

namespace Filee.Setup.Installation;

public static class SetupNames
{
    /// <summary>Winzlee-specific installation GUID; never reuse Filee's Windows uninstall identity.</summary>
    public const string AppGuid = "BC4AA69D-2261-4FC3-9C39-3FB3A2528AE1";

    public const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{" + AppGuid + "}_is1";

    /// <summary>Filee 1.1 and earlier: Velopack installed it per user and registered it here (HKCU).</summary>
    public const string VelopackUninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Filee";

    public const string MachineEnvironmentKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
    public const string UserEnvironmentKey = "Environment";

    public const string AppName = "Winzlee";
    public const string Publisher = "Kylian Provot";
    public const string AppExe = "Winzlee.exe";

    /// <summary>The uninstaller beside Winzlee.exe, used to identify an installed Winzlee copy.</summary>
    public const string UninstallerExe = "uninstall.exe";

    /// <summary>The inventory beside Winzlee.exe: files owned by Winzlee for updates and uninstall.</summary>
    public const string InstallRecordFile = "uninstall.json";

    /// <summary>The "filee" command (cli\filee.cmd), the folder Setup puts on PATH.</summary>
    public const string CliFolder = "cli";

    /// <summary>No legacy Filee Inno Setup files belong to Winzlee; intentionally empty.</summary>
    public static readonly IReadOnlyList<string> InnoUninstallerFiles = [];

    /// <summary>Start menu and desktop shortcut name.</summary>
    public const string ShortcutName = "Winzlee.lnk";

    /// <summary>Registry values Setup adds to the uninstall key (Inno Setup kept the same in "Inno Setup: …").</summary>
    public const string TasksValue = "Winzlee Setup: Tasks";
    public const string LanguageValue = "Winzlee Setup: Language";
    public const string ScopeValue = "Winzlee Setup: Scope";
    public const string InnoTasksValue = "Inno Setup: Selected Tasks";
    public const string InnoAppPathValue = "Inno Setup: App Path";
    public const string InnoVersionValue = "Inno Setup: Setup Version";
    public const string InnoLanguageValue = "Inno Setup: Language";

    /// <summary>Windows 10 1809: the oldest Windows Filee runs on (Inno Setup's MinVersion).</summary>
    public const int MinimumBuild = 17763;

    /// <summary>Where Microsoft offers the Visual C++ Redistributable (x64).</summary>
    public const string VcRedistUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
}
