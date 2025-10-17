using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Dynamic;

using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class MailingService
{
    private readonly GenericDataService _dataService;
    private readonly EmailSettings _emailSettings;

    public MailingService( GenericDataService dataService, EmailSettings emailSettings )
    {
        _dataService = dataService;
        _emailSettings = emailSettings;
    }

    public async Task SendMailAsync(
        TemplatesListModel template,
        List<ExpandoObject> recipients,
        bool isTestMail = false,
        string? testRecipient = null )
    {
        if ( recipients == null || !recipients.Any() )
            return;

        // Als testmail, gebruik alleen eerste 15 recipients
        var recipientsToUse = isTestMail ? recipients.Take(15).ToList() : recipients;

        foreach ( var recipient in recipientsToUse )
        {
            try
            {
                var message = new MimeMessage();

                // From
                message.From.Add( new MailboxAddress( _emailSettings.SenderName, _emailSettings.SenderAddress ) );

                // To
                if ( isTestMail && !string.IsNullOrWhiteSpace( testRecipient ) )
                    message.To.Add( MailboxAddress.Parse( testRecipient ) );
                else
                {
                    var dict = recipient as IDictionary<string, object>;
                    if ( dict != null && dict.ContainsKey( "Email" ) )
                        message.To.Add( MailboxAddress.Parse( dict [ "Email" ]?.ToString() ?? "" ) );
                }

                // Subject & Body
                string subject = ReplaceTemplateFields(template.TemplateSubject, recipient);
                string body = ReplaceTemplateFields(template.TemplateContent ?? "", recipient);

                message.Subject = subject;
                message.Body = new TextPart( "html" ) { Text = body };

                // Verzenden
                using var client = new SmtpClient();
                await client.ConnectAsync( _emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls );
                await client.AuthenticateAsync( _emailSettings.SmtpUser, _emailSettings.SmtpPass );
                await client.SendAsync( message );
                await client.DisconnectAsync( true );

                // Logging kan hier
                Console.WriteLine( $"Mail sent to {( isTestMail ? testRecipient : ( recipient as IDictionary<string, object> )? [ "Email" ] )}" );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Error sending mail: {ex.Message}" );
                // hier eventueel ook log naar database
            }
        }
    }

    private string ReplaceTemplateFields( string templateText, ExpandoObject recipient )
    {
        if ( recipient is not IDictionary<string, object> data )
            return templateText;

        foreach ( var kvp in data )
        {
            string key = "{" + kvp.Key + "}";  // bijvoorbeeld {Firstname}
            string value = kvp.Value?.ToString() ?? "";
            templateText = templateText.Replace( key, value, StringComparison.OrdinalIgnoreCase );
        }
        return templateText;
    }


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
