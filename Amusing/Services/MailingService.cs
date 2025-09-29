using System.Dynamic;

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
        return await _dataService.ExecuteQueryAsync(
        QueryDefinitions.GetEditionsList,
        reader => reader [ "Festival" ].ToString()!
    );
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
                   ListSource = listSourceEnum,
                   ListFilter = reader [ "ListFilter" ].ToString() ?? string.Empty,
                   ListQuery = reader [ "ListQuery" ].ToString() ?? string.Empty
               };
           } );
    }

    public async Task<List<ExpandoObject>> GetDynamicRecipientsAsync( string query )
    {
        List<Dictionary<string, object>> rawList = await _dataService.ExecuteQueryAsync(
        query,
        reader =>
        {
            Dictionary<string, object> row = new( StringComparer.OrdinalIgnoreCase );
            for ( int i = 0; i < reader.FieldCount; i++ )
            {
                string columnName = reader.GetName(i);
                object value = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                row [ columnName ] = value;
            }
            return row;
        } );

        return rawList.Select( ToExpando ).ToList();
    }

    private static ExpandoObject ToExpando( Dictionary<string, object> dict )
    {
        IDictionary<string, object> expando = new ExpandoObject();
        foreach ( KeyValuePair<string, object> kvp in dict )
        {
            expando [ kvp.Key ] = kvp.Value;
        }
        return ( ExpandoObject ) expando;
    }

    public Task<List<RecipientListFilterModel>> GetAllRecipientsAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetFullPersonsList, // Zorg dat je hier je MySQL query in QueryDefinitions hebt staan
            reader => new RecipientListFilterModel
            {
                PersonId = Convert.ToUInt16( reader [ "PersonId" ] ),
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

    public async Task<uint> AddRecipientQueryAsync( RecipientListModel model )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@Name", model.ListName },
            { "@Source", model.ListSource },
            { "@Filter", model.ListFilter },
            { "@Query", model.ListQuery }
        };

        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewRecipientQuery, parameters );
    }

    public async Task UpdateRecipientQueryAsync( RecipientListModel model )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@ListId", model.ListId },
            { "@ListName", model.ListName },
            { "@ListSource", model.ListSource },
            { "@ListFilter", model.ListFilter },
            { "@ListQuery", model.ListQuery }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyRecipientQueryById, parameters );
    }

    public async Task DeleteRecipientQueryAsync( uint queryId )
    {
        Dictionary<string, object> parameters = new()
    {
        { "QueryId", queryId }
    };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteRecipientQuery, parameters );
    }

    public Task<List<TemplatesListModel>> GetMailTemplatesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllEmailTemplates,
           reader => new TemplatesListModel
           {
               TemplateId = Convert.ToUInt32( reader [ "TemplateId" ] ),
               TemplateCreated = reader [ "TemplateCreated" ].ToString() ?? string.Empty,
               TemplateChanged = reader [ "TemplateChanged" ].ToString() ?? string.Empty,
               RecipientListId = Convert.ToUInt32( reader [ "RecipientListId" ] ),
               RecipientListName = reader [ "RecipientListName" ].ToString(),
               RecipientListFilter = reader [ "RecipientListFilter" ].ToString() ?? string.Empty,
               RecipientListQuery = reader [ "RecipientListQuery" ].ToString() ?? string.Empty,
               RecipientListSource = reader [ "RecipientListSource" ].ToString(),
               TemplateName = reader [ "TemplateName" ].ToString(),
               TemplateSubject = reader [ "TemplateSubject" ].ToString(),
               TemplateContent = reader [ "TemplateContent" ].ToString()
           } );
    }

    public async Task<uint> AddTemplateQueryAsync( TemplatesListModel model )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@TemplateName", model.TemplateName },
            { "@TemplateSubject", model.TemplateSubject },
            { "@TemplateContent", model.TemplateContent },
            { "@RecipientListId", model.RecipientListId }
        };

        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewTemplateQuery, parameters );
    }

    public async Task UpdateTemplateQueryAsync( TemplatesListModel model )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@TemplateId", model.TemplateId },
            { "@TemplateName", model.TemplateName },
            { "@TemplateSubject", model.TemplateSubject },
            { "@TemplateContent", model.TemplateContent },
            { "@RecipientListId", model.RecipientListId }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyTemplateQueryById, parameters );
    }

    public async Task DeleteTemplateQueryAsync( uint queryId )
    {
        Dictionary<string, object> parameters = new()
    {
        { "QueryId", queryId }
    };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteTemplateQuery, parameters );
    }
}
