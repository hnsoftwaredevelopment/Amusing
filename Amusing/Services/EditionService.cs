using Amusing.Models;

using MySql.Data.MySqlClient;

namespace Amusing.Services;

public class EditionService
{
    private readonly MySqlConnection _connection;

    public EditionService( IConfiguration configuration )
    {
        _connection = new MySqlConnection( configuration.GetConnectionString( "DefaultConnection" ) );
    }

    public async Task<List<Edition>> GetEditionsAsync()
    {
        List<Edition> editions = new();
        string query = "SELECT festival_id, festivaldatum FROM ah_festivals ORDER BY festivaldatum DESC";

        using MySqlCommand cmd = new(query, _connection);
        await _connection.OpenAsync();
        using System.Data.Common.DbDataReader reader = await cmd.ExecuteReaderAsync();

        while ( await reader.ReadAsync() )
        {
            uint id = Convert.ToUInt32(reader["festival_id"]);

            // Convert DateTime to DateOnly
            DateTime festivalDateTime = Convert.ToDateTime(reader["festivaldatum"]);
            DateOnly festivalDate = DateOnly.FromDateTime(festivalDateTime);

            editions.Add( new Edition
            {
                ID = id.ToString(),
                Text = festivalDate.Year.ToString()
            } );
        }

        await _connection.CloseAsync();
        return editions;
    }
}
