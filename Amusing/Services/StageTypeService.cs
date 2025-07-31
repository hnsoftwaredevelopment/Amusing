using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class StageTypeService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<StageTypeModel>> GetActiveStageTypesListAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetActiveStageTypesList,
           reader => new StageTypeModel
           {
               Type = reader [ "type" ].ToString()
           } );
    }

    public Task<List<StageTypeModel>> GetAllStageTypesListAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllStageTypesList,
           reader => new StageTypeModel
           {
               Type = reader [ "type" ].ToString()
           } );
    }

    public async Task<int> GetNewStageTypeVersionByTypeAsync( string type )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@type", type }
        };

        List<int> result = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetNewStageTypeVersion,
            reader => Convert.ToInt32( reader [ "versie" ] ),
            parameters
        );

        return result.FirstOrDefault();
    }

    public Task<List<StageTypeModel>> GetStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetStageTypes,
    reader => new StageTypeModel
    {
        Type = reader [ "type" ].ToString(),
        Price = Convert.ToDecimal( reader [ "prijs" ] ),
        Description = reader [ "omschrijving" ].ToString()
    } );
    }

    public async Task<bool> DeleteStageTypeAsync( string type, int version )
    {
        Dictionary<string, object> parameters = new()
        {
        { "@type", type },
        { "@version", version }
    };

        int affectedRows = await _dataService.ExecuteNonQueryAsync(QueryDefinitions.DeleteStageType, parameters);

        return affectedRows > 0;
    }

    public Task<List<StageTypeModel>> GetAllStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllStageTypes,
    reader => new StageTypeModel
    {
        Type = reader [ "type" ].ToString(),
        Price = Convert.ToDecimal( reader [ "prijs" ] ),
        Piano = Convert.ToInt32( reader [ "piano" ] ),
        Lectern = Convert.ToInt32( reader [ "lessenaar" ] ),
        Electronics = Convert.ToInt32( reader [ "electra" ] ),
        Drums = Convert.ToInt32( reader [ "drum" ] ),
        GitarAmplifiers = Convert.ToInt32( reader [ "gitaarversterkers" ] ),
        BassAmplifiers = Convert.ToInt32( reader [ "basversterkers" ] ),
        ChoirAmplifiers = Convert.ToInt32( reader [ "koorversterking" ] ),
        Microphones = Convert.ToInt32( reader [ "microfoons" ] ),
        Monitors = Convert.ToInt32( reader [ "monitoren" ] ),
        Speakers = Convert.ToInt32( reader [ "speakers" ] ),
        MixingConsole = Convert.ToInt32( reader [ "mengpaneel" ] ),
        Mp3 = Convert.ToInt32( reader [ "md_mp3" ] ),
        Compatibel = reader [ "compatibel" ].ToString(),
        Description = reader [ "omschrijving" ].ToString(),
        Active = reader.GetInt32( reader.GetOrdinal( "aktief" ) ),
        Version = reader.GetInt32( reader.GetOrdinal( "versie" ) )
    } );
    }

    public async Task InsertStageTypeAsync( StageTypeModel model )
    {
        Dictionary<string, object> parameters = new()
        {
        { "@type", model.Type },
        { "@versie", model.Version },
        { "@prijs", model.Price },
        { "@piano", model.Piano },
        { "@lessenaar", model.Lectern },
        { "@electra", model.Electronics },
        { "@drum", model.Drums },
        { "@gitaarversterkers", model.GitarAmplifiers },
        { "@basversterkers", model.BassAmplifiers },
        { "@koorversterking", model.ChoirAmplifiers },
        { "@microfoons", model.Microphones },
        { "@monitoren", model.Monitors },
        { "@speakers", model.Speakers },
        { "@mengpaneel", model.MixingConsole },
        { "@md_mp3", model.Mp3 },
        { "@compatibel", model.Compatibel ?? "" },
        { "@beschrijving", " " },
        { "@description", " " },
        { "@aktief", model.Active }
    };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.InsertStageType, parameters );
    }

    public async Task<string> GetNextAvailableStageTypeAsync()
    {
        List<StageTypeModel> existing = await GetAllStageTypesListAsync();

        // Verzamel gebruikte hoofdletters
        HashSet<char> used = existing
        .Select( m => m.Type?.Trim().ToUpper() )
        .Where( s => !string.IsNullOrEmpty( s ) && s.Length == 1 && char.IsLetter( s [ 0 ] ) )
        .Select( s => s [ 0 ] )
        .ToHashSet();

        // Controleer op vrije letter van A-Z
        for ( char c = 'A'; c <= 'Z'; c++ )
        {
            if ( !used.Contains( c ) )
            {
                return c.ToString();
            }
        }

        // Fallback: alles is bezet
        throw new InvalidOperationException( "Alle letters van A tot Z zijn al gebruikt als type." );
    }
}
