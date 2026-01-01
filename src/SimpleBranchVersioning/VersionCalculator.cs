using System.Text.RegularExpressions;

namespace SimpleBranchVersioning;

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
    /// Calculates the version string based on branch name and commit ID.
    /// </summary>
    /// <param name="branch">The git branch name.</param>
    /// <param name="commitId">The short commit ID (7 chars).</param>
    /// <returns>The calculated version string.</returns>
    public static string Calculate(string branch, string commitId)
    {
        // Check for release branch pattern
        var match = ReleasePattern.Match(branch);
        if (match.Success)
        {
            return match.Groups["version"].Value;
        }

        // For all other branches: replace / with . and append commit ID
        var normalizedBranch = branch.Replace('/', '.');
        return $"{normalizedBranch}.{commitId}";
    }
}
