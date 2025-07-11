using Amusing.Models;

using MySql.Data.MySqlClient;

namespace Amusing.Services;

public class VolunteerService
{
    private readonly MySqlConnection _connection;

    public VolunteerService( IConfiguration configuration )
    {
        _connection = new MySqlConnection( configuration.GetConnectionString( "DefaultConnection" ) );
    }

    public async Task<List<Volunteer>> GetVolunteersByFestivalIdAsync( uint festivalId )
    {
        List<Volunteer> volunteers = [];

        string query = @"
            select 
        	    vw.festival_id ,
        	    DATE(vw.datum) as 'Datum',
        	    CONCAT_WS(' ', pers.voornaam, pers.tussenvoegsel, pers.achternaam) AS 'Naam',
        	    TIME_FORMAT(vw.beschikbaar_van, '%H:%i') as 'Van',
        	    TIME_FORMAT(vw.beschikbaar_tot, '%H:%i') as 'Tot',
        	    vw.uren_achtereen as 'Uren',
        	    vw.lunch as 'Lunch',
        	    vw.vegetarisch as 'Vegetarisch',
        	    vw.bijeenkomst as 'Bijeenkomst',
        	    vw.ervaring as 'Ervaring',
        	
        	    CASE 
                    WHEN vw.Podiumdienst = 'ja' AND vw.afgehaakt = 'nee' THEN 'ja'
                    ELSE 'nee'
                END AS 'Podiumdienst',
            
        	    CASE 
                    WHEN vw.Podiumdienst = 'nee' AND vw.afgehaakt = 'nee' THEN 'ja'
                    ELSE 'nee'
                END AS 'Overige',
            
                vw.afgehaakt as 'Afgehaakt'
            from amusing.ah_vrijwilligers vw 
            join amusing.ah_personen pers on vw.persoon_id = pers.persoon_id 
            join amusing.ah_festivals fest on vw.festival_id = fest.festival_id 
            where
                vw.festival_id = @festivalId;
        ";

        using MySqlCommand cmd = new(query, _connection);
        cmd.Parameters.AddWithValue( "@festivalId", festivalId );

        await _connection.OpenAsync();
        using System.Data.Common.DbDataReader reader = await cmd.ExecuteReaderAsync();

        while ( await reader.ReadAsync() )
        {
            volunteers.Add( new Volunteer
            {
                FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
                Datum = Convert.ToDateTime( reader [ "Datum" ] ),
                Naam = reader [ "Naam" ].ToString(),
                Van = reader [ "Van" ].ToString(),
                Tot = reader [ "Tot" ].ToString(),
                Uren = Convert.ToInt32( reader [ "Uren" ] ),
                Lunch = reader [ "Lunch" ].ToString().ToLower(),
                Vegetarisch = reader [ "Vegetarisch" ].ToString().ToLower(),
                Bijeenkomst = reader [ "Bijeenkomst" ].ToString().ToLower(),
                Ervaring = reader [ "Ervaring" ].ToString().ToLower(),
                Podiumdienst = reader [ "Podiumdienst" ].ToString().ToLower(),
                Overige = reader [ "Overige" ].ToString().ToLower(),
                Afgehaakt = reader [ "Afgehaakt" ].ToString().ToLower(),
            } );
        }

        await _connection.CloseAsync();
        return volunteers;
    }
}
