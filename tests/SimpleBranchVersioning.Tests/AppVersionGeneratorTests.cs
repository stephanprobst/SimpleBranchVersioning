using Microsoft.CodeAnalysis;
using SimpleBranchVersioning.Tests.Helpers;

namespace SimpleBranchVersioning.Tests;

public class AppVersionGeneratorTests
{
    private const string MinimalSource = """
        namespace TestApp;
        public class Program { }
        """;

    private const string TopLevelSource = """
        System.Console.WriteLine("Hello");
        """;

    #region Basic Generation Tests

    [Test]
    public async Task Generator_GeneratesAppVersionClass()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main");

        await Assert.That(result.GeneratedFileNames).Contains("AppVersion.g.cs");
    }

    [Test]
    public async Task Generator_GeneratesAssemblyVersionInfo()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main");

        await Assert.That(result.GeneratedFileNames).Contains("AssemblyVersionInfo.g.cs");
    }

    [Test]
    public async Task Generator_GeneratesConfigAttribute()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main");

        await Assert.That(result.GeneratedFileNames).Contains("AppVersionConfigAttribute.g.cs");
    }

    [Test]
    public async Task Generator_NoDiagnosticErrors()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main");

        var errors = result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error);

        await Assert.That(errors).IsEmpty();
    }

    #endregion

    #region Release Branch Version Tests

    [Test]
    [Arguments("release/v1.2.3", "1.2.3")]
    [Arguments("release/1.2.3", "1.2.3")]
    [Arguments("release/v0.0.1", "0.0.1")]
    [Arguments("release/v10.20.30", "10.20.30")]
    public async Task Generator_ReleaseBranch_ProducesCorrectVersion(
        string branch, string expectedVersion)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains($"""Version = "{expectedVersion}""");
    }

    [Test]
    [Arguments("release/v1.2.3", "1.2.3+")]
    [Arguments("release/v1.0.0-beta", "1.0.0-beta+")]
    public async Task Generator_ReleaseBranch_PackageVersionIncludesMetadataPrefix(
        string branch, string expectedPrefix)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains($"""PackageVersion = "{expectedPrefix}""");
    }

    [Test]
    [Arguments("release/v1.2.3", "1.2.3.0")]
    [Arguments("release/v0.5.0", "0.5.0.0")]
    public async Task Generator_ReleaseBranch_ProducesCorrectAssemblyVersion(
        string branch, string expectedAssemblyVersion)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains($"""AssemblyVersion = "{expectedAssemblyVersion}""");
    }

    #endregion

    #region Non-Release Branch Tests

    [Test]
    [Arguments("feature/login", "0.0.0-feature.login+")]
    [Arguments("bugfix/issue-42", "0.0.0-bugfix.issue-42+")]
    [Arguments("main", "0.0.0-main+")]
    [Arguments("develop", "0.0.0-develop+")]
    public async Task Generator_NonReleaseBranch_ProducesPrereleaseVersion(
        string branch, string expectedPrefix)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        // Verify prerelease format starts with expected prefix
        await Assert.That(generatedSource).Contains($"""PackageVersion = "{expectedPrefix}""");
        // Also verify this is NOT a release version (starts with 0.0.0)
        await Assert.That(generatedSource).Contains("""PackageVersion = "0.0.0-""");
    }

    [Test]
    [Arguments("feature/login")]
    [Arguments("main")]
    [Arguments("develop")]
    public async Task Generator_NonReleaseBranch_UsesZeroAssemblyVersion(string branch)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""AssemblyVersion = "0.0.0.0""");
    }

    #endregion

    #region All Properties Present Tests

    [Test]
    public async Task Generator_GeneratesAllRequiredProperties()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v1.0.0");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("public const string Version =");
        await Assert.That(generatedSource).Contains("public const string Branch =");
        await Assert.That(generatedSource).Contains("public const string CommitId =");
        await Assert.That(generatedSource).Contains("public const string PackageVersion =");
        await Assert.That(generatedSource).Contains("public const string AssemblyVersion =");
        await Assert.That(generatedSource).Contains("public const string FileVersion =");
        await Assert.That(generatedSource).Contains("public const string InformationalVersion =");
    }

    [Test]
    public async Task Generator_AssemblyVersionInfo_ContainsAllAttributes()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v1.0.0");

        string generatedSource = result.GetRequiredGeneratedSource("AssemblyVersionInfo.g.cs");

        await Assert.That(generatedSource).Contains("AssemblyVersion");
        await Assert.That(generatedSource).Contains("AssemblyFileVersion");
        await Assert.That(generatedSource).Contains("AssemblyInformationalVersion");
    }

    #endregion

    #region Configuration Tests

    [Test]
    public async Task Generator_WithCustomNamespace_UsesConfiguredNamespace()
    {
        const string source = """
            using SimpleBranchVersioning;

            [assembly: AppVersionConfig(Namespace = "MyApp.Version")]

            namespace TestApp;
            public class Program { }
            """;

        var result = GeneratorTestHelper.RunGenerator(
            source,
            branchOverride: "main");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("namespace MyApp.Version");
    }

    [Test]
    public async Task Generator_WithCustomClassName_UsesConfiguredClassName()
    {
        const string source = """
            using SimpleBranchVersioning;

            [assembly: AppVersionConfig(ClassName = "BuildInfo")]

            namespace TestApp;
            public class Program { }
            """;

        var result = GeneratorTestHelper.RunGenerator(
            source,
            branchOverride: "main");

        // Should generate file with custom class name
        await Assert.That(result.GeneratedFileNames).Contains("BuildInfo.g.cs");

        string generatedSource = result.GetRequiredGeneratedSource("BuildInfo.g.cs");

        await Assert.That(generatedSource).Contains("public static class BuildInfo");
    }

    [Test]
    public async Task Generator_WithEmptyNamespace_UsesGlobalNamespace()
    {
        const string source = """
            using SimpleBranchVersioning;

            [assembly: AppVersionConfig(Namespace = "")]

            namespace TestApp;
            public class Program { }
            """;

        var result = GeneratorTestHelper.RunGenerator(
            source,
            branchOverride: "main");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        // Global namespace means no namespace declaration wrapper
        await Assert.That(generatedSource).DoesNotContain("namespace TestApp");
    }

    #endregion

    #region IncludeCommitIdMetadata Tests

    [Test]
    public async Task Generator_WithMetadataDisabled_OmitsCommitIdFromPackageVersion()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v1.2.3",
            includeCommitIdMetadata: false);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""PackageVersion = "1.2.3""");
        // When metadata is disabled, there should be no + in the PackageVersion
        await Assert.That(generatedSource).DoesNotContain("""PackageVersion = "1.2.3+""");
    }

    [Test]
    public async Task Generator_WithMetadataEnabled_IncludesCommitIdInPackageVersion()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v1.2.3",
            includeCommitIdMetadata: true);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""PackageVersion = "1.2.3+""");
    }

    #endregion

    #region Top-Level Statements Tests

    [Test]
    public async Task Generator_WithTopLevelStatements_UsesGlobalNamespace()
    {
        var result = GeneratorTestHelper.RunGenerator(
            TopLevelSource,
            branchOverride: "main");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        // For top-level statements, should use global namespace
        await Assert.That(generatedSource).DoesNotContain("namespace ");
    }

    #endregion

    #region VersionFileWriter Generation Tests

    [Test]
    public async Task Generator_WithGenerateVersionFile_GeneratesVersionFileWriter()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main",
            generateVersionFile: true);

        await Assert.That(result.GeneratedFileNames).Contains("VersionFileWriter.g.cs");
    }

    [Test]
    public async Task Generator_WithSetPackageVersionFromBranch_GeneratesVersionFileWriter()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main",
            setPackageVersionFromBranch: true);

        await Assert.That(result.GeneratedFileNames).Contains("VersionFileWriter.g.cs");
    }

    [Test]
    public async Task Generator_WithBothDisabled_DoesNotGenerateVersionFileWriter()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "main",
            generateVersionFile: false,
            setPackageVersionFromBranch: false);

        await Assert.That(result.GeneratedFileNames).DoesNotContain("VersionFileWriter.g.cs");
    }

    #endregion

    #region Branch Name Slash Replacement Tests

    [Test]
    [Arguments("feature/nested/path", "feature.nested.path")]
    [Arguments("user/john/feature", "user.john.feature")]
    public async Task Generator_ReplacesSlashesInBranchName(
        string branch, string expectedNormalized)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains($"-{expectedNormalized}+");
    }

    #endregion

    #region Git HEAD Content Tests

    [Test]
    [Arguments("ref: refs/heads/main", "main")]
    [Arguments("ref: refs/heads/feature/login", "feature/login")]
    [Arguments("ref: refs/heads/release/v1.2.3", "release/v1.2.3")]
    public async Task Generator_WithGitHeadRef_ExtractsBranchName(
        string headContent, string expectedBranch)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            gitHeadContent: headContent);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains($"""Branch = "{expectedBranch}""");
    }

    [Test]
    public async Task Generator_WithDetachedHead_UsesBranchNameDetached()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            gitHeadContent: "abc1234def5678901234567890abcdef12345678");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""Branch = "detached""");
        await Assert.That(generatedSource).Contains("""CommitId = "abc1234""");
    }

    [Test]
    public async Task Generator_WithBothOverrideAndGitHead_PrefersOverride()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "override-branch",
            gitHeadContent: "ref: refs/heads/git-branch");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""Branch = "override-branch""");
        await Assert.That(generatedSource).DoesNotContain("git-branch");
    }

    [Test]
    public async Task Generator_WithOnlyGitHead_UsesBranchFromHead()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            gitHeadContent: "ref: refs/heads/feature/from-git");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""Branch = "feature/from-git""");
    }

    [Test]
    [Arguments("garbage content")]
    [Arguments("ref: invalid")]
    [Arguments("ref: refs/other/something")]
    public async Task Generator_WithMalformedHead_FallsBackToUnknown(string headContent)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            gitHeadContent: headContent);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""Branch = "unknown""");
    }

    [Test]
    public async Task Generator_WithEmptyHeadContent_FallsBackToUnknown()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            gitHeadContent: "");

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""Branch = "unknown""");
    }

    [Test]
    [Arguments("ref: refs/heads/main\n")]
    [Arguments("ref: refs/heads/main\r\n")]
    [Arguments("  ref: refs/heads/main  ")]
    public async Task Generator_WithHeadContainingWhitespace_TrimsContent(string headContent)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            gitHeadContent: headContent);

        string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

        await Assert.That(generatedSource).Contains("""Branch = "main""");
    }

    #endregion

    #region Diagnostic Tests

    [Test]
    public async Task Generator_ReportsVersionDetectedDiagnostic()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v1.2.3");

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV001", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.Severity).IsEqualTo(DiagnosticSeverity.Info);
    }

    [Test]
    public async Task Generator_VersionDetectedDiagnostic_ContainsVersionInfo()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v2.0.0");

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV001", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNotNull();

        string message = diagnostic!.GetMessage();
        await Assert.That(message).Contains("2.0.0");
        await Assert.That(message).Contains("release/v2.0.0");
    }

    [Test]
    [Arguments("feature/test_underscore", "'_'")]
    [Arguments("feature/user@name", "'@'")]
    [Arguments("feature/test+plus", "'+'")]
    public async Task Generator_InvalidNuGetChars_ReportsSBV002Warning(
        string branch, string expectedInvalidChar)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV002", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains(expectedInvalidChar);
    }

    [Test]
    [Arguments("feature/valid-name")]
    [Arguments("bugfix/issue-42")]
    [Arguments("main")]
    public async Task Generator_ValidNuGetChars_DoesNotReportSBV002(string branch)
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: branch);

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV002", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNull();
    }

    [Test]
    public async Task Generator_ReleaseBranch_DoesNotReportSBV002()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "release/v1.2.3");

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV002", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNull();
    }

    [Test]
    public async Task Generator_ExcessiveBranchLength_ReportsSBV003Warning()
    {
        string longBranch = "feature/" + new string('a', 150);

        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: longBranch);

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV003", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains("128");
    }

    [Test]
    public async Task Generator_NormalBranchLength_DoesNotReportSBV003()
    {
        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: "feature/normal-length-branch");

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV003", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNull();
    }

    [Test]
    public async Task Generator_ReleaseBranch_DoesNotReportSBV003()
    {
        string longReleaseBranch = "release/v1.2.3-" + new string('a', 150);

        var result = GeneratorTestHelper.RunGenerator(
            MinimalSource,
            branchOverride: longReleaseBranch);

        var diagnostic = result.GeneratorDiagnostics
            .FirstOrDefault(d => string.Equals(d.Id, "SBV003", StringComparison.Ordinal));

        await Assert.That(diagnostic).IsNull();
    }

    #endregion
}
