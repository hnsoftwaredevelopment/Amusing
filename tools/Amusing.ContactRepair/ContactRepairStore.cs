using System.Text.Json;
using System.Security.Cryptography;
using MySqlConnector;

namespace Amusing.ContactRepair;

public sealed class ContactRepairStore(string connectionString)
{
    private MySqlConnection Connection() => new(connectionString);
    public async Task<ContactRepairPlan> PlanAsync()
    {
        await using var connection = Connection(); await connection.OpenAsync();
        await using (var readOnly = new MySqlCommand("SET TRANSACTION READ ONLY", connection)) await readOnly.ExecuteNonQueryAsync();
        await using var tx = await connection.BeginTransactionAsync();
        var groups = new List<(uint Id, string Name)>();
        await using (var cmd = new MySqlCommand("""
            SELECT g.zanggroep_id,g.naam FROM ah_zanggroepen g WHERE EXISTS(
                SELECT 1 FROM ah_personen_rollen r WHERE r.zanggroep_id=g.zanggroep_id
                AND r.rol IN ('contactpersoon1','contactpersoon2') GROUP BY r.rol HAVING COUNT(DISTINCT r.persoon_id)>1)
            ORDER BY g.zanggroep_id
            """, connection, tx))
        await using (var reader = await cmd.ExecuteReaderAsync())
            while (await reader.ReadAsync()) groups.Add((reader.GetUInt32(0), reader.GetString(1)));
        var result = new List<ContactRepairGroup>();
        foreach (var group in groups)
        {
            var before = await LinksAsync(connection, tx, group.Id, false);
            var after = ContactPolicy.Normalize(before);
            if (!before.SequenceEqual(after)) result.Add(new(group.Id, group.Name, before, after));
        }
        await tx.CommitAsync();
        return new(connection.Database, DateTime.UtcNow, result);
    }

    public async Task<int> ApplyAsync(ContactRepairPlan plan, string actorUsername)
    {
        await using var connection = Connection(); await connection.OpenAsync();
        if (connection.Database != plan.Database || plan.Groups.Select(g => g.GroupId).Distinct().Count() != plan.Groups.Count)
            throw new InvalidOperationException("Database of groepen komen niet overeen met het plan.");
        await using var tx = await connection.BeginTransactionAsync();
        uint actor;
        await using (var auth = new MySqlCommand("SELECT user_id FROM ah_beheer WHERE username=@username AND NULLIF(TRIM(role),'') IS NOT NULL AND COALESCE(NULLIF(PasswordHash,''),NULLIF(password,'')) IS NOT NULL", connection, tx))
        {
            auth.Parameters.AddWithValue("@username", actorUsername);
            var value = await auth.ExecuteScalarAsync();
            if (value is null or DBNull) throw new UnauthorizedAccessException("Het opgegeven Beheeraccount is niet bevoegd.");
            actor = Convert.ToUInt32(value);
        }
        var planHash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(plan))).ToLowerInvariant();
        foreach (var group in plan.Groups.OrderBy(g => g.GroupId))
        {
            await using (var groupLock = new MySqlCommand("SELECT naam FROM ah_zanggroepen WHERE zanggroep_id=@group AND actief=1 FOR UPDATE", connection, tx))
            {
                groupLock.Parameters.AddWithValue("@group", group.GroupId);
                if ((string?)await groupLock.ExecuteScalarAsync() != group.Name) throw new InvalidOperationException("Koorgegevens zijn gewijzigd of het koor is niet actief.");
            }
            var current = await LinksAsync(connection, tx, group.GroupId, true);
            if (!current.SequenceEqual(ContactPolicy.Ordered(group.Before))) throw new InvalidOperationException("Rollen of accountgegevens zijn gewijzigd. Maak een nieuw plan.");
            if (!ContactPolicy.Normalize(current).SequenceEqual(ContactPolicy.Ordered(group.After))) throw new InvalidOperationException("Het plan voldoet niet aan de goedgekeurde contactregels.");
            await using (var invitations = new MySqlCommand("""
                SELECT id FROM mijn_role_invitations WHERE group_id=@group AND accepted_at IS NULL
                AND cancelled_at IS NULL AND expires_at>UTC_TIMESTAMP(6)
                AND role_name IN ('contactpersoon1','contactpersoon2','overdracht_contactpersoon1') FOR UPDATE
                """, connection, tx))
            {
                invitations.Parameters.AddWithValue("@group", group.GroupId);
                if (await invitations.ExecuteScalarAsync() is not null) throw new InvalidOperationException("Er zijn openstaande contactuitnodigingen. Eerst beoordelen.");
            }
            var added = group.After.Except(group.Before).ToArray();
            var removed = group.Before.Except(group.After).ToArray();
            foreach (var link in added.Concat(removed)) if (!ContactPolicy.IsContact(link.Role)) throw new InvalidOperationException("Een andere rol zou gewijzigd worden.");
            foreach (var link in added) await ChangeAsync("INSERT INTO ah_personen_rollen(persoon_id,zanggroep_id,rol) VALUES(@person,@group,@role)", link);
            foreach (var link in removed) await ChangeAsync("DELETE FROM ah_personen_rollen WHERE persoon_id=@person AND zanggroep_id=@group AND rol=@role", link);
            foreach (var person in added.Concat(removed).Select(l => l.PersonId).Distinct())
            {
                var details = JsonSerializer.Serialize(new { plan = planHash[..16], before = removed.Where(l => l.PersonId == person).Select(l => l.Role), after = added.Where(l => l.PersonId == person).Select(l => l.Role) });
                await using var audit = new MySqlCommand("""
                    INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,changed_fields,target_person_id)
                    VALUES(UTC_TIMESTAMP(6),'beheer',@actor,'ContactRolesNormalized',@group,@details,@person)
                    """, connection, tx);
                audit.Parameters.AddWithValue("@actor", actor); audit.Parameters.AddWithValue("@group", group.GroupId);
                audit.Parameters.AddWithValue("@person", person); audit.Parameters.AddWithValue("@details", details);
                await audit.ExecuteNonQueryAsync();
            }
            if (!(await LinksAsync(connection, tx, group.GroupId, true)).SequenceEqual(ContactPolicy.Ordered(group.After)))
                throw new InvalidOperationException("De nacontrole van de rolkoppelingen is mislukt.");

            async Task ChangeAsync(string sql, ContactLink link)
            {
                await using var cmd = new MySqlCommand(sql, connection, tx);
                cmd.Parameters.AddWithValue("@person", link.PersonId); cmd.Parameters.AddWithValue("@group", group.GroupId); cmd.Parameters.AddWithValue("@role", link.Role);
                if (await cmd.ExecuteNonQueryAsync() != 1) throw new InvalidOperationException("Een rolwijziging kon niet exact worden toegepast.");
            }
        }
        await tx.CommitAsync(); return plan.Groups.Count;
    }

    private static async Task<ContactLink[]> LinksAsync(MySqlConnection connection, MySqlTransaction tx, uint group, bool locked)
    {
        await using var cmd = new MySqlCommand("""
            SELECT r.persoon_id,r.rol,COALESCE(p.email,''),COALESCE(p.actief,0)
            FROM ah_personen_rollen r LEFT JOIN ah_personen p USING(persoon_id)
            WHERE r.zanggroep_id=@group ORDER BY r.persoon_id,r.rol
            """ + (locked ? " FOR UPDATE" : ""), connection, tx);
        cmd.Parameters.AddWithValue("@group", group); var result = new List<ContactLink>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new(reader.GetUInt32(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3)));
        return ContactPolicy.Ordered(result);
    }
}
