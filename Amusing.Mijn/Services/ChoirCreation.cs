using System.ComponentModel.DataAnnotations;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed partial class ChoirProfiles
{
    public async Task<(uint? Id, string? Error)> CreateAsync(uint actor, ChoirProfileInput input, CancellationToken ct)
    {
        if (supportContext?.HttpContext?.User.IsAdministrator() == true)
            return (null, "Log in met een kooraccount om een koor aan te maken.");

        input.Normalize();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), [], true))
            return (null, "Controleer de ingevulde gegevens.");

        await using var connection = await source.OpenConnectionAsync(ct);
        await using var mutex = new MySqlCommand("SELECT GET_LOCK(CONCAT(DATABASE(), ':nieuw-koor'),5)", connection);
        if (Convert.ToInt32(await mutex.ExecuteScalarAsync(ct)) != 1)
            return (null, "Probeer het over enkele seconden opnieuw.");

        try
        {
            await using var tx = await connection.BeginTransactionAsync(ct);
            await using var member = new MySqlCommand("SELECT actief FROM ah_personen WHERE persoon_id=@actor FOR UPDATE", connection, tx);
            member.Parameters.AddWithValue("@actor", actor);
            if (!Convert.ToBoolean(await member.ExecuteScalarAsync(ct)))
                return (null, "Je account is niet actief.");

            await using var references = new MySqlCommand("""
                SELECT EXISTS(SELECT 1 FROM ah_landen WHERE UPPER(code)=@country)
                    AND EXISTS(SELECT 1 FROM ah_genres WHERE genre_id=@genre)
                """, connection, tx);
            references.Parameters.AddWithValue("@country", input.Country);
            references.Parameters.AddWithValue("@genre", input.GenreId);
            if (!Convert.ToBoolean(await references.ExecuteScalarAsync(ct)))
                return (null, "Kies een geldig land en genre.");

            await using var duplicate = new MySqlCommand("""
                SELECT EXISTS(SELECT 1 FROM ah_zanggroepen
                    WHERE LOWER(TRIM(naam))=LOWER(@name) AND LOWER(TRIM(standplaats))=LOWER(@city)
                        AND UPPER(TRIM(land))=@country)
                """, connection, tx);
            duplicate.Parameters.AddWithValue("@name", input.Name);
            duplicate.Parameters.AddWithValue("@city", input.City);
            duplicate.Parameters.AddWithValue("@country", input.Country);
            if (Convert.ToBoolean(await duplicate.ExecuteScalarAsync(ct)))
                return (null, "Er bestaat al een koor met deze naam en standplaats. Vraag de eerste contactpersoon om een uitnodiging of neem contact op met de organisatie.");

            await using var insert = new MySqlCommand("""
                INSERT INTO ah_zanggroepen(naam,standplaats,land,genre_id,beschrijving,rekeningnr,actief)
                VALUES(@name,@city,@country,@genre,'',@iban,1)
                """, connection, tx);
            insert.Parameters.AddWithValue("@name", input.Name);
            insert.Parameters.AddWithValue("@city", input.City);
            insert.Parameters.AddWithValue("@country", input.Country);
            insert.Parameters.AddWithValue("@genre", input.GenreId);
            insert.Parameters.AddWithValue("@iban", input.BankAccount);
            await insert.ExecuteNonQueryAsync(ct);
            var group = checked((uint)insert.LastInsertedId);

            await using var link = new MySqlCommand("""
                INSERT INTO ah_zanggroep_details(id,email) VALUES(@group,@email);
                INSERT INTO ah_personen_rollen(persoon_id,zanggroep_id,rol)
                    VALUES(@actor,@group,'contactpersoon1');
                INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id)
                    VALUES(UTC_TIMESTAMP(6),'choir_person',@actor,'ChoirCreated',@group);
                """, connection, tx);
            link.Parameters.AddWithValue("@group", group);
            link.Parameters.AddWithValue("@email", input.Email ?? "");
            link.Parameters.AddWithValue("@actor", actor);
            await link.ExecuteNonQueryAsync(ct);
            await tx.CommitAsync(ct);
            return (group, null);
        }
        finally
        {
            await using var release = new MySqlCommand("DO RELEASE_LOCK(CONCAT(DATABASE(), ':nieuw-koor'))", connection);
            await release.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }
}
