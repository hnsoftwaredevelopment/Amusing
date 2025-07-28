using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class StageTypeService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<StageTypeModel>> GetStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetStageTypes,
    reader => new StageTypeModel
    {
        Type = reader [ "type" ].ToString(),
        Price = reader [ "prijs" ].ToString(),
        Description = reader [ "omschrijving" ].ToString(),
        Active = reader.GetInt32( reader.GetOrdinal( "aktief" ) )
    } );
    }
    public Task<List<StageTypeModel>> GetAllStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllStageTypes,
    reader => new StageTypeModel
    {
        Type = reader [ "type" ].ToString(),
        Price = reader [ "prijs" ].ToString(),
        Piano = Convert.ToInt32( reader [ "piano" ] ),
        Lectern = Convert.ToInt32( reader [ "lessenaar" ] ),
        Electronics = Convert.ToInt32( reader [ "electra" ] ),
        Drums = Convert.ToInt32( reader [ "drum" ] ),
        GitarEmplifiers = Convert.ToInt32( reader [ "gitaarversterkers" ] ),
        BassEmplifiers = Convert.ToInt32( reader [ "basversterkers" ] ),
        ChoirEmplifiers = Convert.ToInt32( reader [ "koorversterking" ] ),
        Microphones = Convert.ToInt32( reader [ "microfoons" ] ),
        Monitors = Convert.ToInt32( reader [ "monitoren" ] ),
        Speakers = Convert.ToInt32( reader [ "speakers" ] ),
        MixingConsole = Convert.ToInt32( reader [ "mengpaneel" ] ),
        Mp3 = Convert.ToInt32( reader [ "md_mp3" ] ),
        Compatibel = reader [ "compatibel" ].ToString(),
        Active = reader.GetInt32( reader.GetOrdinal( "aktief" ) )
    } );
    }
}
