namespace Amusing.Authentication;

public interface IChoirPasswordStore
{
    Task<string?> ReadHashAsync(uint personId, CancellationToken cancellationToken);
    Task<bool> TryReplaceHashAsync(uint personId, string expectedHash, string replacementHash, CancellationToken cancellationToken);
}
