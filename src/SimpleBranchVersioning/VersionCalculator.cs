using System.Text.RegularExpressions;

namespace SimpleBranchVersioning;

/// <summary>
/// Contains all calculated version information.
/// </summary>
/// <param name="Version">The display version (backward compatible format).</param>
/// <param name="PackageVersion">NuGet-compatible version with optional commit metadata.</param>
/// <param name="AssemblyVersion">Assembly version (X.Y.Z.0 or 0.0.0.0).</param>
/// <param name="FileVersion">File version (same as AssemblyVersion).</param>
/// <param name="InformationalVersion">Full version with all metadata.</param>
public record VersionInfo(
    string Version,
    string PackageVersion,
    string AssemblyVersion,
    string FileVersion,
    string InformationalVersion);

/// <summary>
/// Calculates version strings from git branch and commit information.
/// </summary>
public static class VersionCalculator
{
    private static readonly Regex ReleasePattern = new(
        @"^release/v?(?<version>\d+\.\d+\.\d+.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Calculates all version formats based on branch name and commit ID.
    /// </summary>
    /// <param name="branch">The git branch name.</param>
    /// <param name="commitId">The short commit ID (7 chars).</param>
    /// <param name="includeCommitIdMetadata">Whether to include commit ID as build metadata.</param>
    /// <returns>All calculated version information.</returns>
    public static VersionInfo Calculate(string branch, string commitId, bool includeCommitIdMetadata = true)
    {
        var match = ReleasePattern.Match(branch);

        if (match.Success)
        {
            string semver = match.Groups["version"].Value;

            return new VersionInfo(
                Version: semver,
                PackageVersion: includeCommitIdMetadata ? $"{semver}+{commitId}" : semver,
                AssemblyVersion: $"{semver}.0",
                FileVersion: $"{semver}.0",
                InformationalVersion: includeCommitIdMetadata ? $"{semver}+{commitId}" : semver);
        }

        // Non-release branch
        string normalizedBranch = branch.Replace('/', '.');
        string prereleaseVersion = includeCommitIdMetadata
            ? $"0.0.0-{normalizedBranch}+{commitId}"
            : $"0.0.0-{normalizedBranch}";

        return new VersionInfo(
            Version: $"{normalizedBranch}.{commitId}",
            PackageVersion: prereleaseVersion,
            AssemblyVersion: "0.0.0.0",
            FileVersion: "0.0.0.0",
            InformationalVersion: prereleaseVersion);
    }
}
