namespace SimpleBranchVersioning.Tests;

public class BranchNameValidatorTests
{
    #region ValidateCharacters Tests

    [Test]
    [Arguments("feature.login", false)]
    [Arguments("bugfix.issue-42", false)]
    [Arguments("main", false)]
    [Arguments("feature.nested.path", false)]
    [Arguments("1.2.3-beta", false)]
    public async Task ValidateCharacters_ValidBranchName_ReturnsNoInvalidChars(
        string normalizedBranch, bool expectedHasInvalid)
    {
        var (hasInvalid, _) = BranchNameValidator.ValidateCharacters(normalizedBranch);

        await Assert.That(hasInvalid).IsEqualTo(expectedHasInvalid);
    }

    [Test]
    [Arguments("feature_login", true, "'_'")]
    [Arguments("user@feature", true, "'@'")]
    [Arguments("feature+test", true, "'+'")]
    [Arguments("feature login", true, "' '")]
    public async Task ValidateCharacters_InvalidChars_ReturnsInvalidCharsString(
        string normalizedBranch, bool expectedHasInvalid, string expectedInvalidChars)
    {
        var (hasInvalid, invalidChars) = BranchNameValidator.ValidateCharacters(normalizedBranch);

        await Assert.That(hasInvalid).IsEqualTo(expectedHasInvalid);
        await Assert.That(invalidChars).IsEqualTo(expectedInvalidChars);
    }

    [Test]
    public async Task ValidateCharacters_MultipleInvalidChars_ReturnsAllUniqueChars()
    {
        var (hasInvalid, invalidChars) = BranchNameValidator.ValidateCharacters("feature_test@user_name");

        await Assert.That(hasInvalid).IsTrue();
        await Assert.That(invalidChars).Contains("'_'");
        await Assert.That(invalidChars).Contains("'@'");
    }

    [Test]
    public async Task ValidateCharacters_EmptyString_ReturnsNoInvalidChars()
    {
        var (hasInvalid, invalidChars) = BranchNameValidator.ValidateCharacters("");

        await Assert.That(hasInvalid).IsFalse();
        await Assert.That(invalidChars).IsNull();
    }

    [Test]
    public async Task ValidateCharacters_NullString_ReturnsNoInvalidChars()
    {
        var (hasInvalid, invalidChars) = BranchNameValidator.ValidateCharacters(null!);

        await Assert.That(hasInvalid).IsFalse();
        await Assert.That(invalidChars).IsNull();
    }

    #endregion

    #region IsExcessiveLength Tests

    [Test]
    [Arguments(50, false)]
    [Arguments(100, false)]
    [Arguments(128, false)]
    [Arguments(129, true)]
    [Arguments(200, true)]
    public async Task IsExcessiveLength_VariousLengths_ReturnsExpectedResult(
        int branchLength, bool expectedExcessive)
    {
        string branch = new('a', branchLength);

        bool isExcessive = BranchNameValidator.IsExcessiveLength(branch);

        await Assert.That(isExcessive).IsEqualTo(expectedExcessive);
    }

    [Test]
    public async Task IsExcessiveLength_EmptyString_ReturnsFalse()
    {
        bool isExcessive = BranchNameValidator.IsExcessiveLength("");

        await Assert.That(isExcessive).IsFalse();
    }

    [Test]
    public async Task IsExcessiveLength_NullString_ReturnsFalse()
    {
        bool isExcessive = BranchNameValidator.IsExcessiveLength(null!);

        await Assert.That(isExcessive).IsFalse();
    }

    #endregion

    #region IsReleaseBranch Tests (VersionCalculator)

    [Test]
    [Arguments("release/v1.2.3", true)]
    [Arguments("release/1.2.3", true)]
    [Arguments("release/v0.0.1", true)]
    [Arguments("release/v1.0.0-beta", true)]
    [Arguments("main", false)]
    [Arguments("feature/login", false)]
    [Arguments("release/feature", false)]
    [Arguments("release/main", false)]
    public async Task IsReleaseBranch_ReturnsExpectedResult(string branch, bool expected)
    {
        bool isRelease = VersionCalculator.IsReleaseBranch(branch);

        await Assert.That(isRelease).IsEqualTo(expected);
    }

    #endregion
}
