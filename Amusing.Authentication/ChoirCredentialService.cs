namespace Amusing.Authentication;

/// <summary>Verifies a choir person's credential; does not issue sessions or grant choir access.</summary>
public sealed class ChoirCredentialService(IChoirPasswordStore store, ChoirPasswordVerifier verifier)
{
    public async Task<bool> VerifyAsync(uint personId, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var originalHash = await store.ReadHashAsync(personId, cancellationToken);
        if (originalHash is null) return false;
        var verification = verifier.Verify(password, originalHash);
        cancellationToken.ThrowIfCancellationRequested();
        if (!verification.Valid) return false;
        if (verification.ReplacementHash is null) return true;
        if (await store.TryReplaceHashAsync(personId, originalHash, verification.ReplacementHash, cancellationToken))
            return true;

        // A reset or another login won the update. Never overwrite it or retry indefinitely.
        var currentHash = await store.ReadHashAsync(personId, cancellationToken);
        if (currentHash is null) return false;
        var currentVerification = verifier.Verify(password, currentHash);
        cancellationToken.ThrowIfCancellationRequested();
        return currentVerification.Valid;
    }
}
