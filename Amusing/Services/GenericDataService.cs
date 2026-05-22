using System.Data;
using System.Data.Common;

//using MySql.Data.MySqlClient;
using MySqlConnector;

namespace Amusing.Services;

public class GenericDataService
{
    private readonly MySqlConnection _connection;
    private readonly string? _databaseName;

    public GenericDataService( IConfiguration config )
    {
        string? connectionString = config.GetConnectionString( "DefaultConnection" );

        _connection = new MySqlConnection( connectionString );
        _databaseName = string.IsNullOrWhiteSpace( connectionString )
            ? null
            : new MySqlConnectionStringBuilder( connectionString ).Database;
    }

    public static string UseConfiguredDatabase( string sql, string? databaseName )
    {
        if ( string.IsNullOrWhiteSpace( databaseName )
            || string.Equals( databaseName, "amusing", StringComparison.OrdinalIgnoreCase ) )
        {
            return sql;
        }

        string escapedDatabaseName = databaseName.Replace( "`", "``" );

        return sql
            .Replace( "`amusing`.", $"`{escapedDatabaseName}`.", StringComparison.OrdinalIgnoreCase )
            .Replace( "amusing.", $"`{escapedDatabaseName}`.", StringComparison.OrdinalIgnoreCase );
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(
        string query,
    Func<DbDataReader, T> mapFunc,
    Dictionary<string, object>? parameters = null )
    {
        List<T> results = [];

        await using MySqlConnection connection = new(_connection.ConnectionString);
        await connection.OpenAsync();

        using MySqlCommand cmd = new(UseConfiguredDatabase( query, _databaseName ), connection);

        if ( parameters is not null )
        {
            foreach ( KeyValuePair<string, object> param in parameters )
            {
                cmd.Parameters.AddWithValue( param.Key, param.Value );
            }
        }

        using DbDataReader reader = await cmd.ExecuteReaderAsync();

        while ( await reader.ReadAsync() )
        {
            results.Add( mapFunc( reader ) );
        }

        return results;
    }

    public async Task<int> ExecuteNonQueryAsync(
    string query,
    Dictionary<string, object>? parameters = null )
    {
        await using MySqlConnection connection = new(_connection.ConnectionString);
        await connection.OpenAsync();

        using MySqlCommand cmd = new(UseConfiguredDatabase( query, _databaseName ), connection);

        if ( parameters is not null )
        {
            foreach ( KeyValuePair<string, object> param in parameters )
            {
                cmd.Parameters.AddWithValue( param.Key, param.Value );
            }
        }

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T> ExecuteScalarAsync<T>( string query, Dictionary<string, object>? parameters = null )
    {
        await using MySqlConnection connection = new(_connection.ConnectionString);
        await connection.OpenAsync();

        await using MySqlCommand command = new(UseConfiguredDatabase( query, _databaseName ), connection);

        if ( parameters != null )
        {
            foreach ( KeyValuePair<string, object> param in parameters )
            {
                command.Parameters.AddWithValue( param.Key, param.Value );
            }
        }

        object? result = await command.ExecuteScalarAsync();

        return result != null && result != DBNull.Value
            ? ( T ) Convert.ChangeType( result, typeof( T ) )
            : default!;
    }

    public T? ExecuteScalarQuery<T>( string sql, Dictionary<string, object> parameters )
    {
        using var connection = new MySqlConnection(_connection.ConnectionString);
        using var command = new MySqlCommand(UseConfiguredDatabase( sql, _databaseName ), connection);

        foreach ( var param in parameters )
        {
            command.Parameters.AddWithValue( param.Key, param.Value );
        }

        connection.Open();
        object? result = command.ExecuteScalar();
        connection.Close();

        if ( result == null || result == DBNull.Value )
            return default;

        return ( T ) Convert.ChangeType( result, typeof( T ) );
    }

    /// <summary>
    /// Execute a query and return an open DbDataReader.
    /// Caller is responsible for disposing the reader (which will also close the connection).
    /// Accepts parameters as Dictionary<string, object> for convenience.
    /// </summary>
    public async Task ExecuteReaderAsync(
        string sql,
        Func<DbDataReader, Task> map )
    {
        // Redirect to the overload with parameters = null
        await ExecuteReaderAsync( sql, map, null );
    }

    public async Task ExecuteReaderAsync(
        string sql,
        Func<DbDataReader, Task> map,
        Dictionary<string, object?>? parameters )
    {
        using var connection = new MySqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        using var cmd = new MySqlCommand(UseConfiguredDatabase( sql, _databaseName ), connection);

        // Add parameters when provided
        if ( parameters != null )
        {
            foreach ( var p in parameters )
            {
                cmd.Parameters.AddWithValue( p.Key, p.Value ?? DBNull.Value );
            }
        }

        using var reader = await cmd.ExecuteReaderAsync();

        // Execute the reader callback
        await map( reader );
    }
}
