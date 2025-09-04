using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class MailingService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public enum RecipientListSource
    {
        Unknown = 0,
        Groups,
        Persons
    }

    public async Task<List<string>> GetFestivalListAsync()
    {
        List<EditionModel> _editions = await _dataService.ExecuteQueryAsync( QueryDefinitions.GetEditionsList,
    reader => new EditionModel
    {
        Festival = reader [ "Festival" ].ToString(),
    } );

        return [ .. _editions.Select( e => e.Festival ) ];
    }


    public Task<List<RecipientListModel>> GetRecipientListsAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetAllRecipientLists,
           reader =>
           {
               string? listSourceString = reader [ "ListSource" ] == DBNull.Value
                ? string.Empty
                : reader [ "ListSource" ].ToString()?.Trim();

               RecipientListSource listSourceEnum = listSourceString.ToLower() switch
               {
                   "groups"  => RecipientListSource.Groups,
                   "persons" => RecipientListSource.Persons,
                   _         => RecipientListSource.Unknown
               };

               return new RecipientListModel
               {
                   ListId = Convert.ToUInt32( reader [ "ListId" ] ),
                   ListName = reader [ "ListName" ].ToString() ?? string.Empty,
                   ListCreated = reader [ "ListCreated" ].ToString() ?? string.Empty,
                   ListChanged = reader [ "ListChanged" ].ToString() ?? string.Empty,
                   ListSource = listSourceEnum, // hier zet je de enum
                   ListFilter = reader [ "ListFilter" ].ToString() ?? string.Empty
               };
           } );
    }

    public Task<List<RecipientListFilterModel>> GetAllRecipientsAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetFullPersonsList, // Zorg dat je hier je MySQL query in QueryDefinitions hebt staan
            reader => new RecipientListFilterModel
            {
                Firstname = reader [ "Firstname" ].ToString() ?? string.Empty,
                Lastname = reader [ "Lastname" ].ToString() ?? string.Empty,
                Name = reader [ "Name" ].ToString() ?? string.Empty,
                Email = reader [ "Email" ].ToString() ?? string.Empty,
                Infomailing = Convert.ToBoolean( reader [ "Infomailing" ] ),
                Active = Convert.ToBoolean( reader [ "Active" ] ),
                Role = reader [ "Role" ].ToString() ?? string.Empty,
                GroupName = reader [ "GroupName" ].ToString() ?? string.Empty,
                Festival = reader [ "Festival" ].ToString(),
                StageType = reader [ "StageType" ].ToString() ?? string.Empty,
                Subscribed = Convert.ToBoolean( reader [ "Subscribed" ] ),
                Canceled = Convert.ToBoolean( reader [ "Canceled" ] ),
                Payed = Convert.ToBoolean( reader [ "Payed" ] ),
                Confirmed = Convert.ToBoolean( reader [ "Confirmed" ] ),
                Singers = Convert.ToInt32( reader [ "Singers" ] )
            } );
    }
}
