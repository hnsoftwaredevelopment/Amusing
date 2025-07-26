using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class StageTypeService(GenericDataService dataService)
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<StageTypeModel>> GetStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetStageTypes,
    reader => new StageTypeModel
    {
        Type = reader["type"].ToString(),
        Price = reader["prijs"].ToString(),
        Description = reader["omschrijving"].ToString()
    });
    }
}
