using System.Diagnostics;
using System.Dynamic;

using Amusing.Helpers;
using Amusing.Models;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;

using MimeKit;

namespace Amusing.Services;

public class MailingService
{
    private readonly EmailSettings _emailSettings;
    private readonly IMailingLogger _logger;
    private readonly GenericDataService _dataService;
    private readonly FieldMappingService _mappingService;
    private readonly TransipMailingService _transipMailingService;

    public MailingService( 
        GenericDataService dataService,
        EmailSettings emailSettings,
        FieldMappingService mappingService,
        IMailingLogger logger,
        TransipMailingService transipMailingService )
    {
        _emailSettings = emailSettings;
        _logger = logger;
        _dataService = dataService;
        _mappingService = mappingService;
        _transipMailingService = transipMailingService;
    }

    #region Template Replacement

    private string ReplaceTemplateFields( string templateText, IDictionary<string, object> recipient )
    {
        foreach ( var kvp in recipient )
        {
            string placeholder = "{" + kvp.Key + "}";
            string value = kvp.Value?.ToString() ?? string.Empty;
            templateText = templateText.Replace( placeholder, value, StringComparison.OrdinalIgnoreCase );
        }
        return templateText;
    }

    #endregion

    #region Send Mail Core

    private async Task _SendMimeMessageAsync( MimeMessage message )
    {
        try
        {
            using var client = new SmtpClient();
            var secureOption = _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            Debug.WriteLine( $"SMTP Host: {_emailSettings.SmtpHost}, Port: {_emailSettings.SmtpPort}, SSL: {_emailSettings.EnableSsl}, Using: {secureOption}" );
            Debug.WriteLine( $"User: {_emailSettings.SmtpUser}, PW: {_emailSettings.SmtpPass}, SendMail: {_emailSettings.SenderAddress}" );

            await client.ConnectAsync( _emailSettings.SmtpHost, _emailSettings.SmtpPort, secureOption );
            await client.AuthenticateAsync( _emailSettings.SmtpUser, _emailSettings.SmtpPass );
            await client.SendAsync( message );
            await client.DisconnectAsync( true );

            await _logger.LogMailSentAsync( message.To.ToString(), message.Subject, success: true );
        }
        catch ( Exception smtpEx )
        {
            Debug.WriteLine( $"[MailingService] SMTP failed: {smtpEx}. Trying TransIP fallback..." );
            try
            {
                await _transipMailingService.SendAsync( message.To.ToString(), message.Subject, ( message.Body as TextPart )?.Text ?? "" );
                await _logger.LogMailSentAsync( message.To.ToString(), message.Subject, success: true );
            }
            catch ( Exception transipEx )
            {
                Debug.WriteLine( $"TransIP fallback failed: {transipEx.Message}" );
                await _logger.LogMailSentAsync( message.To.ToString(), message.Subject, success: false, errorMessage: transipEx.Message );
            }
        }
    }

    #endregion

    #region Public Send Methods

    public async Task SendTestMailAsync( TemplatesListModel template, List<ExpandoObject> recipients, string testEmail, int numberToSend = 15 )
    {
        var toSend = recipients.Take(numberToSend);
        foreach ( var recipient in toSend )
        {
            var dict = recipient as IDictionary<string, object>;
            if ( dict == null )
                continue;

            var message = new MimeMessage();
            message.From.Add( new MailboxAddress( _emailSettings.SenderName, _emailSettings.SenderAddress ) );
            message.To.Add( MailboxAddress.Parse( testEmail ) );
            message.Subject = ReplaceTemplateFields( template.TemplateSubject, dict );
            message.Body = new TextPart( "html" ) { Text = ReplaceTemplateFields( template.TemplateContent ?? "", dict ) };

            await _SendMimeMessageAsync( message );
        }
    }

    public async Task SendBulkMailAsync( TemplatesListModel template, List<ExpandoObject> recipients )
    {
        foreach ( var recipient in recipients )
        {
            var dict = recipient as IDictionary<string, object>;
            if ( dict == null || !dict.TryGetValue( "Email", out var emailObj ) || string.IsNullOrWhiteSpace( emailObj?.ToString() ) )
                continue;

            var message = new MimeMessage();
            message.From.Add( new MailboxAddress( _emailSettings.SenderName, _emailSettings.SenderAddress ) );
            message.To.Add( MailboxAddress.Parse( emailObj.ToString()! ) );
            message.Subject = ReplaceTemplateFields( template.TemplateSubject, dict );
            message.Body = new TextPart( "html" ) { Text = ReplaceTemplateFields( template.TemplateContent ?? "", dict ) };

            await _SendMimeMessageAsync( message );
        }
    }

    public async Task<List<(string Recipient, string Subject, string Body)>> GeneratePreviewAsync(
        string subjectTemplate, string bodyTemplate, IEnumerable<IDictionary<string, object>> recipients )
    {
        var result = new List<(string, string, string)>();

        foreach ( var recipient in recipients )
        {
            string subject = ReplaceTemplateFields(subjectTemplate, recipient);
            string body = ReplaceTemplateFields(bodyTemplate, recipient);

            string email = recipient.TryGetValue("Email", out var val) ? val?.ToString() ?? "" : "";
            result.Add( (email, subject, body) );
        }

        return result;
    }

    #endregion

    #region Recipients & Lists

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
                string listSourceStr = reader["ListSource"] == DBNull.Value ? string.Empty : reader["ListSource"].ToString()?.Trim() ?? "";
                var listSourceEnum = listSourceStr.ToLower() switch
                {
                    "groups" => RecipientListSource.Groups,
                    "persons" => RecipientListSource.Persons,
                    _ => RecipientListSource.Unknown
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
        var rawList = await _dataService.ExecuteQueryAsync(query, reader =>
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
                dict[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
            return dict;
        });

        return rawList.Select( ToExpando ).ToList();
    }

    private static ExpandoObject ToExpando( Dictionary<string, object> dict )
    {
        var expando = new ExpandoObject() as IDictionary<string, object>;
        foreach ( var kvp in dict )
            expando [ kvp.Key ] = kvp.Value;
        return ( ExpandoObject ) expando;
    }

    public Task<List<RecipientListFilterModel>> GetAllRecipientsAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetFullPersonsList,
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
        var parameters = new Dictionary<string, object>
        {
            {"@Name", model.ListName},
            {"@Source", model.ListSource},
            {"@Filter", model.ListFilter},
            {"@Query", model.ListQuery}
        };
        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewRecipientQuery, parameters );
    }

    public async Task UpdateRecipientQueryAsync( RecipientListModel model )
    {
        var parameters = new Dictionary<string, object>
        {
            {"@ListId", model.ListId},
            {"@ListName", model.ListName},
            {"@ListSource", model.ListSource},
            {"@ListFilter", model.ListFilter},
            {"@ListQuery", model.ListQuery}
        };
        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyRecipientQueryById, parameters );
    }

    public async Task DeleteRecipientQueryAsync( uint queryId )
    {
        var parameters = new Dictionary<string, object> { { "QueryId", queryId } };
        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteRecipientQuery, parameters );
    }

    #endregion

    #region Templates

    public Task<List<TemplatesListModel>> GetMailTemplatesAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetAllEmailTemplates,
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
        var parameters = new Dictionary<string, object>
        {
            {"@TemplateName", model.TemplateName},
            {"@TemplateSubject", model.TemplateSubject},
            {"@TemplateContent", model.TemplateContent},
            {"@RecipientListId", model.RecipientListId}
        };
        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewTemplateQuery, parameters );
    }

    public async Task UpdateTemplateQueryAsync( TemplatesListModel model )
    {
        var parameters = new Dictionary<string, object>
        {
            {"@TemplateId", model.TemplateId},
            {"@TemplateName", model.TemplateName},
            {"@TemplateSubject", model.TemplateSubject},
            {"@TemplateContent", model.TemplateContent},
            {"@RecipientListId", model.RecipientListId}
        };
        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyTemplateQueryById, parameters );
    }

    public async Task DeleteTemplateQueryAsync( uint queryId )
    {
        var parameters = new Dictionary<string, object> { { "QueryId", queryId } };
        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteTemplateQuery, parameters );
    }

    #endregion
}