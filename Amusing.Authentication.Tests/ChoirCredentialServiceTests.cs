using Amusing.Authentication;
using Xunit;

public class ChoirCredentialServiceTests
{
    private static readonly string Password = string.Concat("Aa0!", Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)));
    private static readonly string OldHash = BCrypt.Net.BCrypt.HashPassword(Password, 10);
    private static readonly string CurrentHash = BCrypt.Net.BCrypt.HashPassword(Password, 11);

    [Fact] public async Task SuccessfulLoginUpgradesOnlyMatchingHash()
    {
        var store = new MemoryStore(OldHash);
        Assert.True(await Service(store).VerifyAsync(42, Password));
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, store.Hash));
        Assert.NotEqual(OldHash, store.Hash);
        Assert.Equal(1, store.Updates);
    }

    [Fact] public async Task WrongPasswordNeverWrites()
    {
        var store = new MemoryStore(OldHash);
        Assert.False(await Service(store).VerifyAsync(42, "wrong"));
        Assert.Equal(0, store.Updates);
        Assert.Equal(OldHash, store.Hash);
    }

    [Theory] [InlineData(null)] [InlineData("broken")]
    public async Task MissingOrInvalidCredentialsFail(string? hash)
    {
        var store = new MemoryStore(hash);
        Assert.False(await Service(store).VerifyAsync(42, Password));
        Assert.Equal(0, store.Updates);
    }

    [Fact] public async Task CurrentHashDoesNotWrite()
    {
        var store = new MemoryStore(CurrentHash);
        Assert.True(await Service(store).VerifyAsync(42, Password));
        Assert.Equal(0, store.Updates);
    }

    [Fact] public async Task ConcurrentPasswordResetRejectsOldPassword()
    {
        var resetHash = BCrypt.Net.BCrypt.HashPassword("Changed-2026!", 11);
        var store = new MemoryStore(OldHash) { ConcurrentHash = () => resetHash };
        Assert.False(await Service(store).VerifyAsync(42, Password));
        Assert.Equal(resetHash, store.Hash);
        Assert.Equal(1, store.Updates);
        Assert.Equal(2, store.Reads);
    }

    [Fact] public async Task ConcurrentUpgradeWithSamePasswordCanLogin()
    {
        var store = new MemoryStore(OldHash) { ConcurrentHash = () => CurrentHash };
        Assert.True(await Service(store).VerifyAsync(42, Password));
        Assert.Equal(CurrentHash, store.Hash);
        Assert.Equal(1, store.Updates);
    }

    [Fact] public async Task ConcurrentDeletionRejectsLogin()
    {
        var store = new MemoryStore(OldHash) { ConcurrentHash = () => null };
        Assert.False(await Service(store).VerifyAsync(42, Password));
        Assert.Null(store.Hash);
    }

    [Fact] public async Task CancellationIsPropagated()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(new MemoryStore(OldHash)).VerifyAsync(42, Password, cancellation.Token));
    }

    private static ChoirCredentialService Service(MemoryStore store) => new(store, new ChoirPasswordVerifier());

    private sealed class MemoryStore(string? hash) : IChoirPasswordStore
    {
        public string? Hash = hash;
        public Func<string?>? ConcurrentHash;
        public int Reads;
        public int Updates;
        public Task<string?> ReadHashAsync(uint personId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(42u, personId);
            Reads++;
            return Task.FromResult(Hash);
        }
        public Task<bool> TryReplaceHashAsync(uint personId, string expectedHash, string replacementHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(42u, personId);
            Updates++;
            if (ConcurrentHash is not null) Hash = ConcurrentHash();
            if (!string.Equals(Hash, expectedHash, StringComparison.Ordinal)) return Task.FromResult(false);
            Hash = replacementHash;
            return Task.FromResult(true);
        }
    }
}
