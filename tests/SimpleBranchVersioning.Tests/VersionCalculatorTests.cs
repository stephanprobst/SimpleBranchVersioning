namespace SimpleBranchVersioning.Tests;

public class VersionCalculatorTests
{
    [Test]
    [Arguments("release/v1.2.3", "abc1234", "1.2.3", "1.2.3+abc1234", "1.2.3.0")]
    [Arguments("release/1.2.3", "abc1234", "1.2.3", "1.2.3+abc1234", "1.2.3.0")]
    [Arguments("release/v0.0.1", "def5678", "0.0.1", "0.0.1+def5678", "0.0.1.0")]
    [Arguments("release/v10.20.30", "ghi9012", "10.20.30", "10.20.30+ghi9012", "10.20.30.0")]
    public async Task Calculate_ReleaseBranch_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion, string expectedAssemblyVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        await Assert.That(result.Version).IsEqualTo(expectedVersion);
        await Assert.That(result.PackageVersion).IsEqualTo(expectedPackageVersion);
        await Assert.That(result.AssemblyVersion).IsEqualTo(expectedAssemblyVersion);
        await Assert.That(result.FileVersion).IsEqualTo(expectedAssemblyVersion);
        await Assert.That(result.InformationalVersion).IsEqualTo(expectedPackageVersion);
    }

    [Test]
    [Arguments("release/v1.2.3-beta", "abc1234", "1.2.3-beta", "1.2.3-beta+abc1234", "1.2.3-beta.0")]
    [Arguments("release/v1.2.3-rc.1", "abc1234", "1.2.3-rc.1", "1.2.3-rc.1+abc1234", "1.2.3-rc.1.0")]
    [Arguments("release/1.0.0-alpha", "def5678", "1.0.0-alpha", "1.0.0-alpha+def5678", "1.0.0-alpha.0")]
    public async Task Calculate_ReleaseBranchWithPrerelease_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion, string expectedAssemblyVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        await Assert.That(result.Version).IsEqualTo(expectedVersion);
        await Assert.That(result.PackageVersion).IsEqualTo(expectedPackageVersion);
        await Assert.That(result.AssemblyVersion).IsEqualTo(expectedAssemblyVersion);
    }

    [Test]
    [Arguments("feature/login", "abc1234", "feature.login.abc1234", "0.0.0-feature.login+abc1234")]
    [Arguments("feature/user-auth", "def5678", "feature.user-auth.def5678", "0.0.0-feature.user-auth+def5678")]
    [Arguments("bugfix/issue-42", "ghi9012", "bugfix.issue-42.ghi9012", "0.0.0-bugfix.issue-42+ghi9012")]
    [Arguments("hotfix/critical", "jkl3456", "hotfix.critical.jkl3456", "0.0.0-hotfix.critical+jkl3456")]
    public async Task Calculate_FeatureOrBugfixBranch_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        await Assert.That(result.Version).IsEqualTo(expectedVersion);
        await Assert.That(result.PackageVersion).IsEqualTo(expectedPackageVersion);
        await Assert.That(result.AssemblyVersion).IsEqualTo("0.0.0.0");
        await Assert.That(result.FileVersion).IsEqualTo("0.0.0.0");
        await Assert.That(result.InformationalVersion).IsEqualTo(expectedPackageVersion);
    }

    [Test]
    [Arguments("main", "abc1234", "main.abc1234", "0.0.0-main+abc1234")]
    [Arguments("master", "def5678", "master.def5678", "0.0.0-master+def5678")]
    [Arguments("develop", "ghi9012", "develop.ghi9012", "0.0.0-develop+ghi9012")]
    public async Task Calculate_MainBranch_ReturnsCorrectVersions(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        await Assert.That(result.Version).IsEqualTo(expectedVersion);
        await Assert.That(result.PackageVersion).IsEqualTo(expectedPackageVersion);
        await Assert.That(result.AssemblyVersion).IsEqualTo("0.0.0.0");
    }

    [Test]
    [Arguments("user/john/feature", "abc1234", "user.john.feature.abc1234", "0.0.0-user.john.feature+abc1234")]
    [Arguments("refs/heads/main", "def5678", "refs.heads.main.def5678", "0.0.0-refs.heads.main+def5678")]
    public async Task Calculate_NestedBranch_ReplacesAllSlashes(
        string branch, string commitId, string expectedVersion, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        await Assert.That(result.Version).IsEqualTo(expectedVersion);
        await Assert.That(result.PackageVersion).IsEqualTo(expectedPackageVersion);
    }

    [Test]
    public async Task Calculate_ReleaseWithoutVersion_TreatedAsNonReleaseBranch()
    {
        var result = VersionCalculator.Calculate("release/feature-x", "abc1234");

        await Assert.That(result.Version).IsEqualTo("release.feature-x.abc1234");
        await Assert.That(result.PackageVersion).IsEqualTo("0.0.0-release.feature-x+abc1234");
        await Assert.That(result.AssemblyVersion).IsEqualTo("0.0.0.0");
    }

    [Test]
    [Arguments("release/v1.2.3", "abc1234", false, "1.2.3")]
    [Arguments("feature/login", "abc1234", false, "0.0.0-feature.login")]
    [Arguments("main", "def5678", false, "0.0.0-main")]
    public async Task Calculate_WithoutMetadata_OmitsCommitId(
        string branch, string commitId, bool includeMetadata, string expectedPackageVersion)
    {
        var result = VersionCalculator.Calculate(branch, commitId, includeMetadata);

        await Assert.That(result.PackageVersion).IsEqualTo(expectedPackageVersion);
        await Assert.That(result.InformationalVersion).IsEqualTo(expectedPackageVersion);
    }

    [Test]
    public async Task Calculate_DefaultIncludesMetadata()
    {
        var result = VersionCalculator.Calculate("release/v1.0.0", "abc1234");

        await Assert.That(result.PackageVersion).IsEqualTo("1.0.0+abc1234");
    }
}
