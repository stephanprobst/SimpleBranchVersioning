using Xunit;

namespace SimpleBranchVersioning.Tests;

public class VersionCalculatorTests
{
    [Theory]
    [InlineData("release/v1.2.3", "abc1234", "1.2.3")]
    [InlineData("release/1.2.3", "abc1234", "1.2.3")]
    [InlineData("release/v0.0.1", "def5678", "0.0.1")]
    [InlineData("release/v10.20.30", "ghi9012", "10.20.30")]
    public void Calculate_ReleaseBranch_ReturnsSemanticVersion(string branch, string commitId, string expected)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("release/v1.2.3-beta", "abc1234", "1.2.3-beta")]
    [InlineData("release/v1.2.3-rc.1", "abc1234", "1.2.3-rc.1")]
    [InlineData("release/1.0.0-alpha", "def5678", "1.0.0-alpha")]
    public void Calculate_ReleaseBranchWithPrerelease_ReturnsFullVersion(string branch, string commitId, string expected)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("feature/login", "abc1234", "feature.login.abc1234")]
    [InlineData("feature/user-auth", "def5678", "feature.user-auth.def5678")]
    [InlineData("bugfix/issue-42", "ghi9012", "bugfix.issue-42.ghi9012")]
    [InlineData("hotfix/critical", "jkl3456", "hotfix.critical.jkl3456")]
    public void Calculate_FeatureOrBugfixBranch_ReturnsNormalizedBranchWithCommit(string branch, string commitId, string expected)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("main", "abc1234", "main.abc1234")]
    [InlineData("master", "def5678", "master.def5678")]
    [InlineData("develop", "ghi9012", "develop.ghi9012")]
    public void Calculate_MainBranch_ReturnsBranchWithCommit(string branch, string commitId, string expected)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("user/john/feature", "abc1234", "user.john.feature.abc1234")]
    [InlineData("refs/heads/main", "def5678", "refs.heads.main.def5678")]
    public void Calculate_NestedBranch_ReplacesAllSlashes(string branch, string commitId, string expected)
    {
        var result = VersionCalculator.Calculate(branch, commitId);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_ReleaseWithoutVersion_TreatedAsRegularBranch()
    {
        var result = VersionCalculator.Calculate("release/feature-x", "abc1234");

        Assert.Equal("release.feature-x.abc1234", result);
    }
}
