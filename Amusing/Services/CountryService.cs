using System.Globalization;

using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class CountryService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<CountryModel>> GetAllCountriesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllCountries, reader =>
        {
            return new CountryModel
            {
                CountryId = reader [ "CountryId" ].ToString()?.ToLower(),
                Country = CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Country" ].ToString()?.ToLower() ?? "" ),
                Active = Convert.ToInt16( reader [ "Active" ] )
            };
        } );
    }

    public Task<List<CountryModel>> GetActiveCountriesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetActiveCountries, reader =>
        {
            return new CountryModel
            {
                CountryId = reader [ "CountryId" ].ToString()?.ToLower(),
                Country = CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Country" ].ToString()?.ToLower() ?? "" ),
            };
        } );
    }
}
