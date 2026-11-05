using System.Data.Common;
namespace Amusing.Authentication.MySql;
public enum ChoirAuditAction { PasswordUpgraded, LoginSucceeded, LoginRejected, LoggedOut, SessionInvalidated }
/// <summary>Append-only events without free-text payloads or credentials.</summary>
public sealed class MySqlChoirAuditLog(DbDataSource source)
{
    public async Task RecordAsync(ChoirAuditAction action, uint? personId, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await InsertAsync(connection, null, action, personId, ct);
    }
    internal static async Task InsertAsync(DbConnection connection, DbTransaction? transaction, ChoirAuditAction action, uint? personId, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO mijn_audit_log (occurred_at,actor_type,actor_id,action) VALUES (UTC_TIMESTAMP(6),@type,@actor,@action)";
        Add(command,"@type",personId.HasValue ? "choir_person" : "anonymous");
        Add(command,"@actor",(object?)personId ?? DBNull.Value);
        Add(command,"@action",action.ToString());
        await command.ExecuteNonQueryAsync(ct);
    }
    private static void Add(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
