namespace Or1nRenameFileNameToDateCreated.Helpers;

/// <summary>
/// Service for generating dynamic application version strings based on system time.
/// Version format: v{YYYY}.{MM}.{DD}.{HH}.{mm}.{ss}.{fff}
/// Example: v2026.02.02.14.35.42.892
/// </summary>
public static class VersionService
{
    /// <summary>
    /// Gets the current application version based on system time.
    /// This is called dynamically each time a version is needed, ensuring it reflects
    /// the actual current time when the app is running.
    /// </summary>
    /// <returns>
    /// A formatted version string with the pattern: v{YYYY}.{MM}.{DD}.{HH}.{mm}.{ss}.{fff}
    /// </returns>
    public static string GetCurrentVersion()
    {
        var now = DateTime.Now;
        return $"v{now:yyyy.MM.dd.HH.mm.ss.fff}";
    }
}
