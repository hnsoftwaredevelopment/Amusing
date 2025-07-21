using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class EditionService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<Edition>> GetEditionsAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetEditions,
           reader =>
           {
               uint id = Convert.ToUInt32(reader["festival_id"]);
               DateTime festivalDateTime = Convert.ToDateTime(reader["festivaldatum"]);
               DateOnly festivalDate = DateOnly.FromDateTime(festivalDateTime);

               return new Edition
               {
                   ID = id.ToString(),
                   Text = festivalDate.Year.ToString()
               };
           } );
    }
}
