using System.Net.Mail;
using System.Security.Claims;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed record MailActor(uint Id, string Type)
{
    public static MailActor? From(ClaimsPrincipal? user) =>
        user?.Identity?.IsAuthenticated == true &&
        uint.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) &&
        user.FindFirstValue("actor_type") is "choir_person" or SupportMode.ActorType
            ? new(id, user.FindFirstValue("actor_type")!)
            : null;

    public async Task<string?> AddressAsync(MySqlDataSource source, CancellationToken ct)
    {
        var query = Type switch
        {
            "choir_person" => "SELECT email FROM ah_personen WHERE persoon_id=@id AND actief=1",
            SupportMode.ActorType => "SELECT herstel_email FROM ah_beheer WHERE user_id=@id AND NULLIF(TRIM(role),'') IS NOT NULL",
            _ => null
        };
        if (query is null) return null;
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", Id);
        var email = (await command.ExecuteScalarAsync(ct) as string)?.Trim();
        return MailAddress.TryCreate(email, out var address) && address.Address == email ? email : null;
    }
}
