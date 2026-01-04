using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SimpleBranchVersioning.Tests.Helpers;

/// <summary>
/// Test implementation of AnalyzerConfigOptionsProvider for configuring build properties.
/// </summary>
public sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly TestAnalyzerConfigOptions _globalOptions;

    public TestAnalyzerConfigOptionsProvider(string editorConfigContent)
    {
        var options = ParseEditorConfig(editorConfigContent);
        _globalOptions = new TestAnalyzerConfigOptions(options);
    }

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => TestAnalyzerConfigOptions.Empty;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => TestAnalyzerConfigOptions.Empty;

    private static Dictionary<string, string> ParseEditorConfig(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string line in content.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            int equalsIndex = trimmed.IndexOf('=', StringComparison.Ordinal);
            if (equalsIndex > 0)
            {
                string key = trimmed[..equalsIndex].Trim();
                string value = trimmed[(equalsIndex + 1)..].Trim();
                result[key] = value;
            }
        }

        return result;
    }

    private sealed class TestAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
    {
        public static readonly TestAnalyzerConfigOptions Empty =
            new(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => options.TryGetValue(key, out value);
    }
}
