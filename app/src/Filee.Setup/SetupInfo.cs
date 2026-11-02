using System.Reflection;

namespace Filee.Setup;

/// <summary>Public project metadata shown by the Windows installer.</summary>
public static class SetupInfo
{
    /// <summary>Winzlee's repository, defined in Directory.Build.props for normal builds.</summary>
    public static string RepositoryUrl =>
        typeof(SetupInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "RepositoryUrl")?.Value ?? "https://github.com/KylianProvot/Winzlee";

    /// <summary>Winzlee's home and download page (no redirects to the upstream Filee website).</summary>
    public const string WebsiteUrl = "https://github.com/KylianProvot/Winzlee";

    /// <summary>The app version embedded in Setup; the payload manifest identifies the installed files.</summary>
    public static string Version =>
        typeof(SetupInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.0.0";
}
