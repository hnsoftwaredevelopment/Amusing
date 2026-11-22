using System.Globalization;
using System.Text.RegularExpressions;
namespace Amusing.Authentication;
public sealed record PasswordVerification(bool Valid, string? ReplacementHash = null);
public sealed class ChoirPasswordVerifier
{
    private const int TargetCost = 11;
    public PasswordVerification Verify(string password, string hash)
    {
        if (password is null || hash is null || !Regex.IsMatch(hash, @"\A\$2[aby]\$(0[4-9]|[12][0-9]|3[01])\$[./A-Za-z0-9]{53}\z"))
            return new(false);
        try
        {
            if (!BCrypt.Net.BCrypt.Verify(password, hash)) return new(false);
        }
        catch (BCrypt.Net.SaltParseException) { return new(false); }
        var cost = int.Parse(hash.AsSpan(4, 2), CultureInfo.InvariantCulture);
        return new(true, cost < TargetCost ? BCrypt.Net.BCrypt.HashPassword(password, TargetCost) : null);
    }
}
