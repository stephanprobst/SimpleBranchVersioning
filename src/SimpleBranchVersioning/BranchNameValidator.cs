using System.Text.RegularExpressions;

namespace SimpleBranchVersioning;

/// <summary>
/// Validates branch names for NuGet version compatibility.
/// </summary>
public static class BranchNameValidator
{
    /// <summary>
    /// Maximum recommended branch name length for practical version strings.
    /// </summary>
    public const int MaxBranchLength = 128;

    // NuGet prerelease identifiers allow: [0-9A-Za-z-]
    // After slash→dot normalization, dots are also valid
    // This pattern matches any character that is NOT valid
    private static readonly Regex InvalidNuGetCharsPattern = new(
        @"[^0-9A-Za-z.\-]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Validates that a normalized branch name contains only NuGet-compatible characters.
    /// </summary>
    /// <param name="normalizedBranch">Branch name with slashes replaced by dots.</param>
    /// <returns>
    /// A tuple containing whether invalid characters were found and a string of the invalid characters.
    /// </returns>
    public static (bool HasInvalidChars, string? InvalidChars) ValidateCharacters(string normalizedBranch)
    {
        if (string.IsNullOrEmpty(normalizedBranch))
        {
            return (false, null);
        }

        var matches = InvalidNuGetCharsPattern.Matches(normalizedBranch);
        if (matches.Count == 0)
        {
            return (false, null);
        }

        // Collect unique invalid characters
        // Cast required for .NET Standard 2.0 compatibility (MatchCollection doesn't implement IEnumerable<Match>)
        var invalidChars = matches
            .Cast<Match>()
            .Select(m => m.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(c => c, StringComparer.Ordinal);

        return (true, string.Join(", ", invalidChars.Select(c => $"'{c}'")));
    }

    /// <summary>
    /// Checks if a branch name exceeds the recommended maximum length.
    /// </summary>
    /// <param name="branch">The original branch name.</param>
    /// <returns>True if the branch name is excessively long.</returns>
    public static bool IsExcessiveLength(string branch) => !string.IsNullOrEmpty(branch) && branch.Length > MaxBranchLength;
}
