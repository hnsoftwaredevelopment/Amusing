using System.Text.Json;
using Amusing.Authentication;
using Xunit;
public class PasswordTests
{
    [Fact] public void PhpPasswordsUpgradeAndRemainValid()
    {
        var fixtures = JsonSerializer.Deserialize<List<Fixture>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"fixtures.json")))!;
        foreach (var f in fixtures)
        {
            var result = new ChoirPasswordVerifier().Verify(f.password, f.hash);
            Assert.True(result.Valid);
            Assert.NotNull(result.ReplacementHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(f.password, result.ReplacementHash));
            Assert.Null(new ChoirPasswordVerifier().Verify(f.password, result.ReplacementHash).ReplacementHash);
            Assert.False(new ChoirPasswordVerifier().Verify("wrong", f.hash).Valid);
        }
    }
    [Theory][InlineData(11)][InlineData(12)] public void DoesNotDowngrade(int cost)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Test-2026!", cost);
        var result = new ChoirPasswordVerifier().Verify("Test-2026!", hash);
        Assert.True(result.Valid); Assert.Null(result.ReplacementHash);
    }
    [Theory][InlineData("")][InlineData("broken")][InlineData("$2y$99$invalid")]
    public void InvalidHashesAreRejected(string hash) => Assert.False(new ChoirPasswordVerifier().Verify("Test",hash).Valid);
    private record Fixture(string name,string password,string hash);
}
