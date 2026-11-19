using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class PersonalContactInput : IValidatableObject
{
    [Required, StringLength(40)] public string FirstName { get; set; } = "";
    [StringLength(20)] public string? Prefix { get; set; }
    [Required, StringLength(50)] public string LastName { get; set; } = "";
    [StringLength(15)] public string? Phone { get; set; }
    [StringLength(15)] public string? Mobile { get; set; }
    public string Email { get; set; } = "";

    public static bool PhoneValid(string value) =>
        Regex.IsMatch(value.Trim(), @"^[+0][1-9][0-9\- ]{7,14}$", RegexOptions.CultureInvariant);

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (!string.IsNullOrWhiteSpace(Phone) && !PhoneValid(Phone))
            yield return new("Vul een geldig vast telefoonnummer in.", [nameof(Phone)]);
        if (!string.IsNullOrWhiteSpace(Mobile) && !PhoneValid(Mobile))
            yield return new("Vul een geldig mobiel nummer in.", [nameof(Mobile)]);
    }
}

public sealed class PersonalContacts(MySqlDataSource source)
{
    public async Task<PersonalContactInput?> ReadAsync(uint actor, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        return await ReadAsync(connection, null, actor, ct);
    }

    private static async Task<PersonalContactInput?> ReadAsync(MySqlConnection connection, MySqlTransaction? transaction,
        uint actor, CancellationToken ct)
    {
        await using var command = new MySqlCommand("""
            SELECT p.voornaam, p.tussenvoegsel, p.achternaam, c.telefoon_vast, c.telefoon_mobiel, p.email
            FROM ah_personen p LEFT JOIN ah_contactgegevens c USING (persoon_id)
            WHERE p.persoon_id=@actor AND p.actief=1
            """, connection, transaction);
        command.Parameters.AddWithValue("@actor", actor);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        string Text(int column) => reader.IsDBNull(column) ? "" : reader.GetString(column);
        return new() { FirstName = Text(0), Prefix = Text(1), LastName = Text(2), Phone = Text(3), Mobile = Text(4), Email = Text(5) };
    }

    public async Task<bool> SaveAsync(uint actor, PersonalContactInput input, CancellationToken ct)
    {
        if (!Validator.TryValidateObject(input, new ValidationContext(input), [], true)) return false;
        var first = input.FirstName.Trim();
        var prefix = input.Prefix?.Trim() ?? "";
        var last = input.LastName.Trim();
        var phone = input.Phone?.Trim() ?? "";
        var mobile = input.Mobile?.Trim() ?? "";

        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using var account = new MySqlCommand("SELECT actief FROM ah_personen WHERE persoon_id=@actor FOR UPDATE", connection, tx);
        account.Parameters.AddWithValue("@actor", actor);
        if (!Convert.ToBoolean(await account.ExecuteScalarAsync(ct))) return false;

        var previous = await ReadAsync(connection, tx, actor, ct);
        if (previous is null) return false;

        var fields = new List<string>();
        if (previous.FirstName != first) fields.Add("FirstName");
        if (previous.Prefix != prefix) fields.Add("Prefix");
        if (previous.LastName != last) fields.Add("LastName");
        if (previous.Phone != phone) fields.Add("Phone");
        if (previous.Mobile != mobile) fields.Add("Mobile");
        if (fields.Count == 0) return true;

        await using var command = new MySqlCommand("""
            UPDATE ah_personen SET voornaam=@first,tussenvoegsel=@prefix,achternaam=@last WHERE persoon_id=@actor;
            INSERT INTO ah_contactgegevens(persoon_id,telefoon_vast,telefoon_mobiel)
            VALUES(@actor,@phone,@mobile)
            ON DUPLICATE KEY UPDATE telefoon_vast=VALUES(telefoon_vast),telefoon_mobiel=VALUES(telefoon_mobiel);
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,changed_fields)
            VALUES(UTC_TIMESTAMP(6),'choir_person',@actor,'PersonalContactChanged',@fields);
            """, connection, tx);
        command.Parameters.AddWithValue("@actor", actor);
        command.Parameters.AddWithValue("@first", first);
        command.Parameters.AddWithValue("@prefix", prefix);
        command.Parameters.AddWithValue("@last", last);
        command.Parameters.AddWithValue("@phone", phone);
        command.Parameters.AddWithValue("@mobile", mobile);
        command.Parameters.AddWithValue("@fields", string.Join(",", fields));
        await command.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }
}
