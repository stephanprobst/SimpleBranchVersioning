using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SimpleBranchVersioning.Tests.Helpers;

/// <summary>
/// Helper for running source generator tests with configurable options.
/// </summary>
public static class GeneratorTestHelper
{
    /// <summary>
    /// Runs the AppVersionGenerator and returns the generated outputs.
    /// </summary>
    public static GeneratorTestResult RunGenerator(
        string sourceCode,
        string? branchOverride = null,
        string? gitHeadContent = null,
        string? rootNamespace = null,
        bool includeCommitIdMetadata = true,
        bool generateVersionFile = false,
        bool setPackageVersionFromBranch = true)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Use Basic.Reference.Assemblies for compilation references
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: Basic.Reference.Assemblies.Net80.References.All,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Build editorconfig content for global options
        string editorConfig = BuildEditorConfig(
            branchOverride,
            rootNamespace,
            includeCommitIdMetadata,
            generateVersionFile,
            setPackageVersionFromBranch);

        // Create additional texts (git HEAD simulation)
        ImmutableArray<AdditionalText> additionalTexts = [];
        if (gitHeadContent != null)
        {
            additionalTexts = [new InMemoryAdditionalText(".git/HEAD", gitHeadContent)];
        }

        // Configure driver with options
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new AppVersionGenerator().AsSourceGenerator()],
            additionalTexts: additionalTexts,
            parseOptions: (CSharpParseOptions)syntaxTree.Options,
            optionsProvider: new TestAnalyzerConfigOptionsProvider(editorConfig));

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        return new GeneratorTestResult(
            outputCompilation,
            diagnostics,
            driver.GetRunResult());
    }

    private static string BuildEditorConfig(
        string? branchOverride,
        string? rootNamespace,
        bool includeCommitIdMetadata,
        bool generateVersionFile,
        bool setPackageVersionFromBranch)
    {
        List<string> lines =
        [
            "is_global = true",
            $"build_property.IncludeCommitIdMetadata = {includeCommitIdMetadata.ToString().ToLowerInvariant()}",
            $"build_property.GenerateVersionFile = {generateVersionFile.ToString().ToLowerInvariant()}",
            $"build_property.SetPackageVersionFromBranch = {setPackageVersionFromBranch.ToString().ToLowerInvariant()}"
        ];

        if (branchOverride != null)
        {
            lines.Add($"build_property.SimpleBranchVersioning_Branch = {branchOverride}");
        }

        if (rootNamespace != null)
        {
            lines.Add($"build_property.RootNamespace = {rootNamespace}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}

/// <summary>
/// Result from running the source generator.
/// </summary>
public sealed record GeneratorTestResult(
    Compilation OutputCompilation,
    ImmutableArray<Diagnostic> Diagnostics,
    GeneratorDriverRunResult DriverRunResult)
{
    /// <summary>
    /// Gets the generated source trees (excluding the original input).
    /// </summary>
    public IEnumerable<SyntaxTree> GeneratedTrees => OutputCompilation.SyntaxTrees.Skip(1);

    /// <summary>
    /// Gets the content of a generated file by hint name. Throws if not found.
    /// </summary>
    public string GetRequiredGeneratedSource(string hintName)
    {
        var result = DriverRunResult.Results.FirstOrDefault();
        if (result.GeneratedSources.IsDefaultOrEmpty)
        {
            throw new InvalidOperationException(
                $"Generated source '{hintName}' was not found. Available: {string.Join(", ", GeneratedFileNames)}");
        }

        var source = result.GeneratedSources
            .FirstOrDefault(s => string.Equals(s.HintName, hintName, StringComparison.Ordinal));

        return source.SourceText.ToString();
    }

    /// <summary>
    /// Gets all generated source file names.
    /// </summary>
    public IEnumerable<string> GeneratedFileNames
        => DriverRunResult.Results
            .SelectMany(r => r.GeneratedSources)
            .Select(s => s.HintName);

    /// <summary>
    /// Gets diagnostics reported by the generator.
    /// </summary>
    public IEnumerable<Diagnostic> GeneratorDiagnostics
        => DriverRunResult.Results
            .SelectMany(r => r.Diagnostics);
}
