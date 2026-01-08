using SimpleBranchVersioning.Tests.Helpers;

namespace SimpleBranchVersioning.Tests;

/// <summary>
/// Integration tests for AppVersionGenerator that test real file I/O paths.
/// These tests create actual git directory structures on disk.
/// </summary>
public class AppVersionGeneratorIntegrationTests
{
    private const string MinimalSource = """
        namespace TestApp;
        public class Program { }
        """;

    #region ParseGitInfo Integration Tests

    [Test]
    public async Task Generator_WithRealGitDirectory_ReadsCommitFromRefsHeads()
    {
        // Arrange
        string tempDir = CreateTempDirectory();
        try
        {
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "heads"));

            // Create HEAD file pointing to a branch
            string headPath = Path.Combine(gitDir, "HEAD");
            await File.WriteAllTextAsync(headPath, "ref: refs/heads/main\n");

            // Create refs/heads/main with a commit hash
            string refPath = Path.Combine(gitDir, "refs", "heads", "main");
            await File.WriteAllTextAsync(refPath, "abc1234def5678901234567890123456789012345\n");

            // Act
            var result = GeneratorTestHelper.RunGeneratorWithGitDirectory(
                MinimalSource,
                headPath);

            string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

            // Assert
            await Assert.That(generatedSource).Contains("""Branch = "main""");
            await Assert.That(generatedSource).Contains("""CommitId = "abc1234""");
        }
        finally
        {
            DeleteTempDirectory(tempDir);
        }
    }

    [Test]
    public async Task Generator_WithRealGitDirectory_ReadsCommitFromNestedBranch()
    {
        // Arrange
        string tempDir = CreateTempDirectory();
        try
        {
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "heads", "feature"));

            // Create HEAD file pointing to a nested branch
            string headPath = Path.Combine(gitDir, "HEAD");
            await File.WriteAllTextAsync(headPath, "ref: refs/heads/feature/login\n");

            // Create refs/heads/feature/login with a commit hash
            string refPath = Path.Combine(gitDir, "refs", "heads", "feature", "login");
            await File.WriteAllTextAsync(refPath, "def5678abc1234567890123456789012345678901\n");

            // Act
            var result = GeneratorTestHelper.RunGeneratorWithGitDirectory(
                MinimalSource,
                headPath);

            string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

            // Assert
            await Assert.That(generatedSource).Contains("""Branch = "feature/login""");
            await Assert.That(generatedSource).Contains("""CommitId = "def5678""");
        }
        finally
        {
            DeleteTempDirectory(tempDir);
        }
    }

    [Test]
    public async Task Generator_WithRealGitDirectory_HandlesDetachedHead()
    {
        // Arrange
        string tempDir = CreateTempDirectory();
        try
        {
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);

            // Create HEAD file with a direct commit hash (detached HEAD)
            string headPath = Path.Combine(gitDir, "HEAD");
            await File.WriteAllTextAsync(headPath, "9876543210abcdef1234567890abcdef12345678\n");

            // Act
            var result = GeneratorTestHelper.RunGeneratorWithGitDirectory(
                MinimalSource,
                headPath);

            string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

            // Assert
            await Assert.That(generatedSource).Contains("""Branch = "detached""");
            await Assert.That(generatedSource).Contains("""CommitId = "9876543""");
        }
        finally
        {
            DeleteTempDirectory(tempDir);
        }
    }

    [Test]
    public async Task Generator_WithRealGitDirectory_MissingRefFile_UsesBranchWithoutCommitId()
    {
        // Arrange
        string tempDir = CreateTempDirectory();
        try
        {
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "heads"));

            // Create HEAD file pointing to a branch
            string headPath = Path.Combine(gitDir, "HEAD");
            await File.WriteAllTextAsync(headPath, "ref: refs/heads/main\n");

            // Note: NOT creating refs/heads/main file - simulates missing ref

            // Act
            var result = GeneratorTestHelper.RunGeneratorWithGitDirectory(
                MinimalSource,
                headPath);

            string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

            // Assert - branch is extracted but commit ID falls back to "0000000"
            await Assert.That(generatedSource).Contains("""Branch = "main""");
            await Assert.That(generatedSource).Contains("""CommitId = "0000000""");
        }
        finally
        {
            DeleteTempDirectory(tempDir);
        }
    }

    [Test]
    public async Task Generator_WithRealGitDirectory_ShortCommitHash_UsesBranchWithoutCommitId()
    {
        // Arrange
        string tempDir = CreateTempDirectory();
        try
        {
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "heads"));

            // Create HEAD file pointing to a branch
            string headPath = Path.Combine(gitDir, "HEAD");
            await File.WriteAllTextAsync(headPath, "ref: refs/heads/main\n");

            // Create refs/heads/main with a too-short commit hash (< 7 chars)
            string refPath = Path.Combine(gitDir, "refs", "heads", "main");
            await File.WriteAllTextAsync(refPath, "abc12\n");

            // Act
            var result = GeneratorTestHelper.RunGeneratorWithGitDirectory(
                MinimalSource,
                headPath);

            string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

            // Assert - branch is extracted but commit ID falls back to "0000000" due to short hash
            await Assert.That(generatedSource).Contains("""Branch = "main""");
            await Assert.That(generatedSource).Contains("""CommitId = "0000000""");
        }
        finally
        {
            DeleteTempDirectory(tempDir);
        }
    }

    [Test]
    public async Task Generator_WithRealGitDirectory_ReleaseBranch_ExtractsVersion()
    {
        // Arrange
        string tempDir = CreateTempDirectory();
        try
        {
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "heads", "release"));

            // Create HEAD file pointing to a release branch
            string headPath = Path.Combine(gitDir, "HEAD");
            await File.WriteAllTextAsync(headPath, "ref: refs/heads/release/v1.2.3\n");

            // Create refs/heads/release/v1.2.3 with a commit hash
            string refPath = Path.Combine(gitDir, "refs", "heads", "release", "v1.2.3");
            await File.WriteAllTextAsync(refPath, "abc1234def5678901234567890123456789012345\n");

            // Act
            var result = GeneratorTestHelper.RunGeneratorWithGitDirectory(
                MinimalSource,
                headPath);

            string generatedSource = result.GetRequiredGeneratedSource("AppVersion.g.cs");

            // Assert
            await Assert.That(generatedSource).Contains("""Branch = "release/v1.2.3""");
            await Assert.That(generatedSource).Contains("""Version = "1.2.3""");
            await Assert.That(generatedSource).Contains("""CommitId = "abc1234""");
            await Assert.That(generatedSource).Contains("""PackageVersion = "1.2.3+abc1234""");
        }
        finally
        {
            DeleteTempDirectory(tempDir);
        }
    }

    #endregion

    #region Helper Methods

    private static string CreateTempDirectory()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"sbv-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    private static void DeleteTempDirectory(string tempDir)
    {
        try
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    #endregion
}
