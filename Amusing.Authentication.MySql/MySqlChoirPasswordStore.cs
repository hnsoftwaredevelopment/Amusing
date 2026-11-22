using System.Data.Common;

namespace Amusing.Authentication.MySql;

/// <summary>MySQL/MariaDB storage. Supply a MySqlConnector DbDataSource owned by the host.</summary>
public sealed class MySqlChoirPasswordStore(DbDataSource dataSource) : IChoirPasswordStore
{
    public async Task<string?> ReadHashAsync(uint personId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT hash FROM ah_personen_wachtwoorden WHERE id = @id";
        AddParameter(command, "@id", personId);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? null : (string)value;
    }

    public async Task<bool> TryReplaceHashAsync(uint personId, string expectedHash, string replacementHash, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        // The legacy column has a case-insensitive collation; hashes must match byte-for-byte.
        command.CommandText = "UPDATE ah_personen_wachtwoorden SET hash = @replacementHash WHERE id = @id AND BINARY hash = BINARY @expectedHash";
        AddParameter(command, "@id", personId);
        AddParameter(command, "@expectedHash", expectedHash);
        AddParameter(command, "@replacementHash", replacementHash);
        var updated = await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        if (updated)
            await MySqlChoirAuditLog.InsertAsync(connection, transaction, ChoirAuditAction.PasswordUpgraded, personId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
