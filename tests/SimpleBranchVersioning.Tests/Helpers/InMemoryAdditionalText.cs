using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SimpleBranchVersioning.Tests.Helpers;

/// <summary>
/// In-memory implementation of AdditionalText for testing.
/// </summary>
public sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    public override string Path => path;

    public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
}
