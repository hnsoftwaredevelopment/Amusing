using Amusing.Services;

using Xunit;

namespace Beheer.Tests;

public class PersonPasswordGeneratorTests
{
    [Fact]
    public void GenerateTemporaryPassword_CreatesLegacyCompatiblePassword()
    {
        string password = PersonPasswordGenerator.GenerateTemporaryPassword();

        Assert.Equal(10, password.Length);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => PersonPasswordGenerator.SpecialCharacters.Contains(c));
    }
}
