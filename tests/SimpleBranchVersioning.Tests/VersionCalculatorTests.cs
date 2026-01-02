using Xunit;

namespace SimpleBranchVersioning.Tests;

public class VersionCalculatorTests
{
    [Theory]
    [InlineData("release/v1.2.3", "abc1234", "1.2.3", "1.2.3+abc1234", "1.2.3.0")]
    [InlineData("release/1.2.3", "abc1234", "1.2.3", "1.2.3+abc1234", "1.2.3.0")]
    [InlineData("release/v0.0.1", "def5678", "0.0.1", "0.0.1+def5678", "0.0.1.0")]
    [InlineData("release/v10.20.30", "ghi9012", "10.20.30", "10.20.30+ghi9012", "10.20.30.0")]
    public void Calculate_ReleaseBranch_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion, string expectedAssemblyVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expectedVersion, result.Version);
        Assert.Equal(expectedPackageVersion, result.PackageVersion);
        Assert.Equal(expectedAssemblyVersion, result.AssemblyVersion);
        Assert.Equal(expectedAssemblyVersion, result.FileVersion);
        Assert.Equal(expectedPackageVersion, result.InformationalVersion);
    }

    [Theory]
    [InlineData("release/v1.2.3-beta", "abc1234", "1.2.3-beta", "1.2.3-beta+abc1234", "1.2.3-beta.0")]
    [InlineData("release/v1.2.3-rc.1", "abc1234", "1.2.3-rc.1", "1.2.3-rc.1+abc1234", "1.2.3-rc.1.0")]
    [InlineData("release/1.0.0-alpha", "def5678", "1.0.0-alpha", "1.0.0-alpha+def5678", "1.0.0-alpha.0")]
    public void Calculate_ReleaseBranchWithPrerelease_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion, string expectedAssemblyVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expectedVersion, result.Version);
        Assert.Equal(expectedPackageVersion, result.PackageVersion);
        Assert.Equal(expectedAssemblyVersion, result.AssemblyVersion);
    }

    [Theory]
    [InlineData("feature/login", "abc1234", "feature.login.abc1234", "0.0.0-feature.login+abc1234")]
    [InlineData("feature/user-auth", "def5678", "feature.user-auth.def5678", "0.0.0-feature.user-auth+def5678")]
    [InlineData("bugfix/issue-42", "ghi9012", "bugfix.issue-42.ghi9012", "0.0.0-bugfix.issue-42+ghi9012")]
    [InlineData("hotfix/critical", "jkl3456", "hotfix.critical.jkl3456", "0.0.0-hotfix.critical+jkl3456")]
    public void Calculate_FeatureOrBugfixBranch_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expectedVersion, result.Version);
        Assert.Equal(expectedPackageVersion, result.PackageVersion);
        Assert.Equal("0.0.0.0", result.AssemblyVersion);
        Assert.Equal("0.0.0.0", result.FileVersion);
        Assert.Equal(expectedPackageVersion, result.InformationalVersion);
    }

    [Theory]
    [InlineData("main", "abc1234", "main.abc1234", "0.0.0-main+abc1234")]
    [InlineData("master", "def5678", "master.def5678", "0.0.0-master+def5678")]
    [InlineData("develop", "ghi9012", "develop.ghi9012", "0.0.0-develop+ghi9012")]
    public void Calculate_MainBranch_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expectedVersion, result.Version);
        Assert.Equal(expectedPackageVersion, result.PackageVersion);
        Assert.Equal("0.0.0.0", result.AssemblyVersion);
    }

    [Theory]
    [InlineData("user/john/feature", "abc1234", "user.john.feature.abc1234", "0.0.0-user.john.feature+abc1234")]
    [InlineData("refs/heads/main", "def5678", "refs.heads.main.def5678", "0.0.0-refs.heads.main+def5678")]
    public void Calculate_NestedBranch_ReplacesAllSlashes(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expectedVersion, result.Version);
        Assert.Equal(expectedPackageVersion, result.PackageVersion);
    }

    [Fact]
    public void Calculate_ReleaseWithoutVersion_TreatedAsNonReleaseBranch()
    {
        var result = VersionCalculator.Calculate("release/feature-x", "abc1234");

        Assert.Equal("release.feature-x.abc1234", result.Version);
        Assert.Equal("0.0.0-release.feature-x+abc1234", result.PackageVersion);
        Assert.Equal("0.0.0.0", result.AssemblyVersion);
    }

    [Theory]
    [InlineData("release/v1.2.3", "abc1234", false, "1.2.3")]
    [InlineData("feature/login", "abc1234", false, "0.0.0-feature.login")]
    [InlineData("main", "def5678", false, "0.0.0-main")]
    public void Calculate_WithoutMetadata_OmitsCommitId(
        string branch, string commitId, bool includeMetadata, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId, includeMetadata);

        Assert.Equal(expectedPackageVersion, result.PackageVersion);
        Assert.Equal(expectedPackageVersion, result.InformationalVersion);
    }

    [Fact]
    public void Calculate_DefaultIncludesMetadata()
    {
        var result = VersionCalculator.Calculate("release/v1.0.0", "abc1234");

        Assert.Equal("1.0.0+abc1234", result.PackageVersion);
    }
}
