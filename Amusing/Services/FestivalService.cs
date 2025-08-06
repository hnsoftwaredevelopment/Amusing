using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class FestivalService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<FestivalModel>> GetFestivalOverviewAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetFestivals,
    reader => new FestivalModel
    {
        FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
        Festival = reader [ "Festival" ].ToString(),
        Festivaldatum = DateOnly.FromDateTime( reader.GetDateTime( reader.GetOrdinal( "Datum" ) ) ),
        Gepubliceerd = reader [ "Gepubliceerd" ].ToString()
    } );
    }

    public Task<List<FestivalModel>> GetFestivalDataAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetFestivalData,
    reader => new FestivalModel
    {
        FestivalId = Convert.ToUInt32( reader [ "FestivalId" ] ),
        Festival = reader [ "Festival" ].ToString(),
        Festivaldatum = DateOnly.FromDateTime( reader.GetDateTime( reader.GetOrdinal( "Datum" ) ) ),
        StartInschrijving = DateOnly.FromDateTime( reader.GetDateTime( reader.GetOrdinal( "StartInschrijving" ) ) ),
        EindeInschrijving = DateOnly.FromDateTime( reader.GetDateTime( reader.GetOrdinal( "EindeInschrijving" ) ) ),
        Wachtlijst = Convert.ToInt16( reader [ "Wachtlijst" ] ),
        PubliceerPlanning = Convert.ToInt16( reader [ "PubliceerPlanning" ] ),
        MinutenTussenOptredens = Convert.ToUInt16( reader [ "MinutenTussenOptredens" ] ),
        MaximumMinutenTussenOptredens = Convert.ToUInt16( reader [ "MaximumMinutenTussenOptredens" ] ),
        MaximumUrenVrijwilligers = Convert.ToUInt16( reader [ "MaximumUrenVrijwilligers" ] ),
        BoeteOnderbrekingOptredens = Convert.ToDecimal( reader [ "BoeteOnderbrekingOptredens" ] ),
        StartVrijwilligersTaken = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "StartVrijwilligersTaken" ] ),
        EindeVrijwilligersTaken = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "EindeVrijwilligersTaken" ] ),
        StartVrijwilligersPauze = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "StartVrijwilligersPauze" ] ),
        EindeVrijwilligersPauze = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "EindeVrijwilligersPauze" ] ),
        EindeVasteVrijwilligersTaken = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "EindeVasteVrijwilligersTaken" ] ),
        Aktief = Convert.ToInt16( reader [ "Aktief" ] )
    } );
    }

    public async Task ModifyFestivalAsync( FestivalModel model )
    {
        Dictionary<string, object> parameters = new()
            {
                { "@festivalid", model.FestivalId },
                { "@Festival", model.Festival },
                { "@Festivaldatum", model.Festivaldatum },
                { "@StartInschrijving", model.StartInschrijving },
                { "@EindeInschrijving", model.EindeInschrijving },
                { "@Wachtlijst", model.Wachtlijst },
                { "@PubliceerPlanning", model.PubliceerPlanning },
                { "@StartVrijwilligersTaken", model.StartVrijwilligersTaken },
                { "@EindeVrijwilligersTaken", model.EindeVrijwilligersTaken },
                { "@StartVrijwilligersPauze", model.StartVrijwilligersPauze },
                { "@EindeVrijwilligersPauze", model.EindeVrijwilligersPauze },
                { "@EindeVasteVrijwilligersTaken", model.EindeVasteVrijwilligersTaken }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyFestival, parameters );
    }

    public async Task ModifyConditionAsync( FestivalModel model )
    {
        Dictionary<string, object> parameters = new()
            {
                { "@festivalid", model.FestivalId },
                { "@MinutenTussenOptredens", model.MinutenTussenOptredens },
                { "@MaximumMinutenTussenOptredens", model.MaximumMinutenTussenOptredens },
                { "@MaximumUrenVrijwilligers", model.MaximumUrenVrijwilligers },
                { "@BoeteOnderbrekingOptredens", model.BoeteOnderbrekingOptredens }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyCondition, parameters );
    }

    public async Task<bool> DeleteFestivalAsync( int festivalId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@festivalid", festivalId }
        };

        int affectedRows = await _dataService.ExecuteNonQueryAsync(QueryDefinitions.DeleteFestival, parameters);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteConditionAsync( int festivalId )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@festivalid", festivalId }
    };

        int affectedRows = await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteCondition, parameters );

        return affectedRows > 0;
    }
}