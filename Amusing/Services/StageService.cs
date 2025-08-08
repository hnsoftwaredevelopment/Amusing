using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class StageService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<StageModel>> GetAllStagesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllStages, reader => new StageModel
        {
            PodiumId = Convert.ToInt32( reader [ "Podium-Id" ] ),
            Naam = reader [ "Naam" ].ToString(),
            Soort = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Bi/Bu" ].ToString().ToLower() ),
            Type = reader [ "Type" ].ToString().ToUpper(),
            Kwaliteit = Convert.ToInt32( reader [ "Kwaliteit" ] ),
            MaxZangers = Convert.ToInt32( reader [ "Max. zangers" ] ),
            Vrijwilligers = reader [ "Vrijwilligers" ].ToString().Equals( "geen", StringComparison.OrdinalIgnoreCase ) ? "0" : reader [ "Vrijwilligers" ].ToString(),
            Start = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Start" ] ),
            Eind = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Eind" ] ),
            Van = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Van" ] ),
            Tot = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Tot" ] ),
            KaartNummer = Convert.ToInt32( reader [ "Kaart-Id" ] ),
            Aktief = Convert.ToInt32( reader [ "Kaart-Id" ] ) < 1 ? 0 : 1
        } );
    }

    public Task<List<StageModel>> GetActiveStagesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetActiveStages, reader => new StageModel
        {
            PodiumId = Convert.ToInt32( reader [ "Podium-Id" ] ),
            Naam = reader [ "Naam" ].ToString(),
            Soort = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Bi/Bu" ].ToString().ToLower() ),
            Type = reader [ "Type" ].ToString().ToUpper(),
            Kwaliteit = Convert.ToInt32( reader [ "Kwaliteit" ] ),
            MaxZangers = Convert.ToInt32( reader [ "Max. zangers" ] ),
            Vrijwilligers = reader [ "Vrijwilligers" ].ToString().Equals( "geen", StringComparison.OrdinalIgnoreCase ) ? "0" : reader [ "Vrijwilligers" ].ToString(),
            Start = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Start" ] ),
            Eind = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Eind" ] ),
            Van = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Van" ] ),
            Tot = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Tot" ] ),
            KaartNummer = Convert.ToInt32( reader [ "Kaart-Id" ] )
        } );
    }
    public Task<List<StageModel>> GetInActiveStagesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetInActiveStages, reader => new StageModel
        {
            PodiumId = Convert.ToInt32( reader [ "Podium-Id" ] ),
            Naam = reader [ "Naam" ].ToString(),
            Soort = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Bi/Bu" ].ToString().ToLower() ),
            Type = reader [ "Type" ].ToString().ToUpper(),
            Kwaliteit = Convert.ToInt32( reader [ "Kwaliteit" ] ),
            MaxZangers = Convert.ToInt32( reader [ "Max. zangers" ] ),
            Vrijwilligers = reader [ "Vrijwilligers" ].ToString().Equals( "geen", StringComparison.OrdinalIgnoreCase ) ? "0" : reader [ "Vrijwilligers" ].ToString(),
            Start = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Start" ] ),
            Eind = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Eind" ] ),
            Van = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Van" ] ),
            Tot = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Tot" ] )
        } );
    }
}
