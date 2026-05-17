using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class StageService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<StageModel>> GetAllStagesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllStages, reader =>
        {
            // Safely read Kaart-Id
            int kaartNummer = reader["Kaart-Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Kaart-Id"]);

            return new StageModel
            {
                PodiumId = Convert.ToUInt16( reader [ "Podium-Id" ] ),
                Naam = reader [ "Naam" ]?.ToString(),
                Soort = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Bi/Bu" ]?.ToString().ToLower() ?? "" ),
                Type = reader [ "Type" ]?.ToString().ToUpper(),
                Kwaliteit = Convert.ToInt32( reader [ "Kwaliteit" ] ),
                MaxZangers = Convert.ToInt32( reader [ "Max. zangers" ] ),
                Vrijwilligers = Convert.ToInt32( reader [ "Vrijwilligers" ]?.ToString().Equals( "geen", StringComparison.OrdinalIgnoreCase ) == true
                    ? "0"
                    : reader [ "Vrijwilligers" ]?.ToString() ),
                Start = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Start" ] ),
                Eind = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Eind" ] ),
                Van = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Van" ] ),
                Tot = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Tot" ] ),
                KaartNummer = kaartNummer,
                Aktief = kaartNummer > 0 ? 1 : 0
            };
        } );
    }

    public Task<List<StageModel>> GetActiveStagesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetActiveStages, reader => new StageModel
        {
            PodiumId = Convert.ToUInt16( reader [ "Podium-Id" ] ),
            Naam = reader [ "Naam" ].ToString(),
            Soort = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Bi/Bu" ].ToString().ToLower() ),
            Type = reader [ "Type" ].ToString().ToUpper(),
            Kwaliteit = Convert.ToInt32( reader [ "Kwaliteit" ] ),
            MaxZangers = Convert.ToInt32( reader [ "Max. zangers" ] ),
            Vrijwilligers = Convert.ToInt32( reader [ "Vrijwilligers" ].ToString().Equals( "geen", StringComparison.OrdinalIgnoreCase ) ? "0" : reader [ "Vrijwilligers" ].ToString() ),
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
            PodiumId = Convert.ToUInt16( reader [ "Podium-Id" ] ),
            Naam = reader [ "Naam" ].ToString(),
            Soort = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Bi/Bu" ].ToString().ToLower() ),
            Type = reader [ "Type" ].ToString().ToUpper(),
            Kwaliteit = Convert.ToInt32( reader [ "Kwaliteit" ] ),
            MaxZangers = Convert.ToInt32( reader [ "Max. zangers" ] ),
            Vrijwilligers = Convert.ToInt32( reader [ "Vrijwilligers" ].ToString().Equals( "geen", StringComparison.OrdinalIgnoreCase ) ? "0" : reader [ "Vrijwilligers" ].ToString() ),
            Start = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Start" ] ),
            Eind = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Optredens Eind" ] ),
            Van = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Van" ] ),
            Tot = TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "Vrijwilligers Tot" ] )
        } );
    }

    public async Task ModifyStageAsync( StageModel model )
    {
        Dictionary<string, object> parameters = new()
            {
                { "@PodiumId", model.PodiumId },
                { "@Naam", model.Naam },
                { "@Soort", model.Soort },
                { "@Type", model.Type },
                { "@Kwaliteit", model.Kwaliteit },
                { "@MaxZangers", model.MaxZangers },
                { "@AantalVrijwilligers", model.Vrijwilligers },
                { "@Opening", model.Start.ToTimeSpan() },
                { "@Sluiting", model.Eind.ToTimeSpan() },
                { "@VrijwilligersVanaf", model.Van.ToTimeSpan() },
                { "@VrijwilligersTot", model.Tot.ToTimeSpan() },
                { "@KaartNummer", model.KaartNummer }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyStage, parameters );
    }

    public async Task<uint> InsertNewStageAsync()
    {
        return await _dataService.ExecuteScalarAsync<uint>(
            QueryDefinitions.InsertNewStage
        );
    }

    public async Task<bool> DeleteStageAsync( uint stageId )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@StageId", stageId }
    };

        int affectedRows = await _dataService.ExecuteNonQueryAsync(QueryDefinitions.DeleteStage, parameters);

        return affectedRows > 0;
    }
}
