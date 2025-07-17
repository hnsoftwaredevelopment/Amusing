using Amusing.Models;

using MySql.Data.MySqlClient;

namespace Amusing.Services;

public class EmailAddressesService
{
    private readonly MySqlConnection _connection;

    public EmailAddressesService( IConfiguration configuration )
    {
        _connection = new MySqlConnection( configuration.GetConnectionString( "DefaultConnection" ) );
    }

    public async Task<List<EmailAddressesModel>> GetEmailAddressesAsync()
    {
        List<EmailAddressesModel> emailaddresses = [];

        string query = @"
            select distinct  
    	        grp.naam as Groep,
	            CONCAT_WS(' ', pers.voornaam, pers.tussenvoegsel, pers.achternaam) AS 'Naam' ,
	            pers.email as 'E-Mail',
                grp.land as Land
            from amusing.ah_personen pers
                join amusing.ah_personen_rollen prol on pers.persoon_id = prol.persoon_id 
                join amusing.ah_zanggroepen grp on prol.zanggroep_id = grp.zanggroep_id
            where pers.infomailing = 1;
        ";

        using MySqlCommand cmd = new(query, _connection);

        await _connection.OpenAsync();
        using System.Data.Common.DbDataReader reader = await cmd.ExecuteReaderAsync();

        while ( await reader.ReadAsync() )
        {
            emailaddresses.Add( new EmailAddressesModel
            {
                Groep = reader [ "Groep" ].ToString(),
                Naam = reader [ "Naam" ].ToString(),
                Email = reader [ "E-Mail" ].ToString(),
                Land = reader [ "Land" ].ToString().ToLower(),
            } );
        }

        await _connection.CloseAsync();
        return emailaddresses;
    }
}
