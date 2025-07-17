using Amusing.Models;

using MySql.Data.MySqlClient;

namespace Amusing.Services;

public class RegistrationService
{
    private readonly MySqlConnection _connection;

    public RegistrationService( IConfiguration configuration )
    {
        _connection = new MySqlConnection( configuration.GetConnectionString( "DefaultConnection" ) );
    }

    public async Task<List<RegistrationModel>> GetRegistrationsByFestivalIdAsync( uint festivalId )
    {
        List<RegistrationModel> registrations = [];

        string query = @"
            select
                i.festival_id as festival_id,
                i.ingeschreven as Datum,
                grp.naam as Naam,
                grp.standplaats as Stad,
                i.podiumsoort as Podium,
                i.aantal_deelnemers as Zangers,
                gen.nl as Genre,
                ((case
                    i.podiumsoort when 'B' then 25.00
                    when 'C' then 50.00
                    when 'D' then 75.00
                    else 0.00
                end) + (case
                    when (i.aantal_deelnemers between 1 and 20) then 50.00
                    when (i.aantal_deelnemers between 21 and 50) then 75.00
                    when (i.aantal_deelnemers between 51 and 100) then 100.00
                    when (i.aantal_deelnemers > 100) then 125.00
                    else 0.00
                end)) as TeBetalen,
                if((i.betaald is null), 'Nee', 'Ja') as Betaald,
                if((i.bevestigd is null), 'Nee', 'Ja') as Bevestigd,
                i.nfve as Kleedkamer,
                i.binnenoptredens as Binnen,
                i.buitenoptredens as Buiten
            from
                amusing.ah_inschrijvingen i
                left join amusing.ah_zanggroepen grp on i.zanggroep_id = grp.zanggroep_id
                left join amusing.ah_genres gen on grp.genre_id = gen.genre_id
            where
                i.festival_id = @festivalId
            order by
                grp.naam;
        ";

        using MySqlCommand cmd = new(query, _connection);
        cmd.Parameters.AddWithValue( "@festivalId", festivalId );

        await _connection.OpenAsync();
        using System.Data.Common.DbDataReader reader = await cmd.ExecuteReaderAsync();

        while ( await reader.ReadAsync() )
        {
            registrations.Add( new RegistrationModel
            {
                FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
                Datum = Convert.ToDateTime( reader [ "Datum" ] ),
                Naam = reader [ "Naam" ].ToString(),
                Stad = reader [ "Stad" ].ToString(),
                Podium = reader [ "Podium" ].ToString(),
                Zangers = Convert.ToInt32( reader [ "Zangers" ] ),
                Genre = reader [ "Genre" ].ToString(),
                TeBetalen = Convert.ToDecimal( reader [ "TeBetalen" ] ),
                Betaald = reader [ "Betaald" ].ToString(),
                Bevestigd = reader [ "Bevestigd" ].ToString(),
                Kleedkamer = reader [ "Kleedkamer" ].ToString(),
                Binnen = Convert.ToInt32( reader [ "Binnen" ] ),
                Buiten = Convert.ToInt32( reader [ "Buiten" ] )
            } );
        }

        await _connection.CloseAsync();
        return registrations;
    }
}
