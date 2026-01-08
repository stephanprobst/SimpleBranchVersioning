using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SimpleBranchVersioning.Tests.Helpers;

/// <summary>
/// File-based implementation of AdditionalText for integration tests.
/// Reads content from actual files on disk.
/// </summary>
public sealed class FileBasedAdditionalText(string filePath) : AdditionalText
{
    public override string Path => filePath;

    public override SourceText? GetText(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        string content = File.ReadAllText(filePath);
        return SourceText.From(content);
    }
}
