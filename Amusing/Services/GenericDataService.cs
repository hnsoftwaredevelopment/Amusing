using System.Data.Common;

//using MySql.Data.MySqlClient;
using MySqlConnector;

namespace Amusing.Services;

public class GenericDataService
{
    private readonly MySqlConnection _connection;

    public GenericDataService( IConfiguration config )
    {
        _connection = new MySqlConnection( config.GetConnectionString( "DefaultConnection" ) );
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(
        string query,
    Func<DbDataReader, T> mapFunc,
    Dictionary<string, object>? parameters = null )
    {
        List<T> results = [];

        await using MySqlConnection connection = new(_connection.ConnectionString);
        await connection.OpenAsync();

        using MySqlCommand cmd = new(query, connection);

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
}
