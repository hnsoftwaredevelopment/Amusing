using Amusing.Models;
using Amusing.Services;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Xunit;

namespace Beheer.Tests;

public class MailTemplatePersistenceTests : IAsyncLifetime
{
    private string _connection="";
    public Task InitializeAsync()
    {
        _connection=Environment.GetEnvironmentVariable("BEHEER_MAIL_TEST_CONNECTION")??"";
        if(_connection.Length>0 && !new MySqlConnectionStringBuilder(_connection).Database.StartsWith("mijn_test_",StringComparison.Ordinal))
            throw new InvalidOperationException("Only isolated test schemas allowed.");
        return Task.CompletedTask;
    }
    public Task DisposeAsync()=>Task.CompletedTask;
    private async Task Execute(string sql)
    {
        await using var db=new MySqlConnection(_connection);await db.OpenAsync();
        await using var cmd=new MySqlCommand(sql,db);await cmd.ExecuteNonQueryAsync();
    }
    private async Task<MailingService> TemplatePersistence()
    {
        await Execute("""
            DROP TABLE IF EXISTS ah_mailing_templates; DROP TABLE IF EXISTS ah_recipient_lists;
            CREATE TABLE ah_recipient_lists(id INT UNSIGNED PRIMARY KEY,name VARCHAR(80),source VARCHAR(80),filter TEXT,query TEXT);
            CREATE TABLE ah_mailing_templates(
                id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                changed DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                recipientlist INT UNSIGNED NULL,name VARCHAR(80) NOT NULL,subject VARCHAR(80) NOT NULL,
                content TEXT,templatesubject VARCHAR(100),templatecontent TEXT,
                FOREIGN KEY(recipientlist) REFERENCES ah_recipient_lists(id));
            INSERT INTO ah_mailing_templates(id,name,subject,content,templatesubject,templatecontent,changed)
            VALUES(1,'Test sjabloon','Oud','Oude inhoud','Oud','Oude inhoud','2020-01-01');
            """);
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["ConnectionStrings:DefaultConnection"]=Environment.GetEnvironmentVariable("BEHEER_MAIL_TEST_CONNECTION")
        }).Build();
        return new(new GenericDataService(config),new EmailSettings(),new FieldMappingService(),null!);
    }

    [QueueDatabaseFact]
    public async Task TemplateSaveReturnsPersistedTextAndCurrentTimestampWithoutMutatingTheDraft()
    {
        var service=await TemplatePersistence();
        var draft=new TemplatesListModel {TemplateId=1,TemplateName="Test sjabloon",TemplateSubject="Hallo {Firstname}",
            TemplateContent="<p>Nieuwe <strong>inhoud</strong></p>",TemplateChanged="2020-01-01",RecipientListId=0};
        var saved=await service.SaveTemplateAsync(draft);
        Assert.Equal(draft.TemplateSubject,saved.TemplateSubject);
        Assert.Equal(draft.TemplateContent,saved.TemplateContent);
        Assert.NotEqual("2020-01-01",saved.TemplateChanged);
        Assert.DoesNotContain("2020",saved.TemplateChanged!);
        Assert.Equal("2020-01-01",draft.TemplateChanged);
    }

    [QueueDatabaseFact]
    public async Task TemplateSaveDoesNotReportSuccessForADeletedTemplate()
    {
        var service=await TemplatePersistence();
        var draft=new TemplatesListModel {TemplateId=999,TemplateName="Verdwenen",TemplateSubject="Hallo",TemplateContent="<p>Tekst</p>"};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.SaveTemplateAsync(draft));
    }
    [QueueDatabaseFact] public async Task NewTemplateWithoutRecipientListIsStoredAndCanBeUpdated()
    {
        var service=await TemplatePersistence();
        var draft=new TemplatesListModel{TemplateName="Nieuw",TemplateSubject="Hallo {Firstname}",TemplateContent="<p>Deutsch, Nederlands en €</p>",RecipientListId=0};
        var saved=await service.SaveTemplateAsync(draft);
        Assert.True(saved.TemplateId>1);Assert.Equal(0u,draft.TemplateId);
        Assert.Equal(draft.TemplateContent,saved.TemplateContent);Assert.False(string.IsNullOrEmpty(saved.TemplateChanged));
        saved.TemplateContent="<p>Bijgewerkt</p>";
        var updated=await service.SaveTemplateAsync(saved);
        Assert.Equal(saved.TemplateId,updated.TemplateId);Assert.Equal(saved.TemplateContent,updated.TemplateContent);
        Assert.Equal(2,(await service.GetMailTemplatesAsync()).Count);
    }
    [QueueDatabaseFact] public async Task RealRecipientListSurvivesSaving()
    {
        var service=await TemplatePersistence();
        await Execute("INSERT INTO ah_recipient_lists(id,name) VALUES(2,'Testlijst')");
        var saved=await service.SaveTemplateAsync(new(){TemplateId=1,TemplateName="Test sjabloon",TemplateSubject="Onderwerp",TemplateContent="<p>Tekst</p>",RecipientListId=2});
        Assert.Equal(2u,saved.RecipientListId);
    }
    [QueueDatabaseFact] public async Task FailedWriteLeavesTheEditableSnapshotIntact()
    {
        var service=await TemplatePersistence();
        await Execute("ALTER TABLE ah_mailing_templates ADD CONSTRAINT reject_content CHECK(templatecontent NOT LIKE '%REJECT%')");
        var draft=new TemplatesListModel{TemplateId=1,TemplateName="Test sjabloon",TemplateSubject="Onderwerp",TemplateContent="<p>REJECT</p>"};
        await Assert.ThrowsAsync<MySqlException>(()=>service.SaveTemplateAsync(draft));
        Assert.Equal("<p>REJECT</p>",draft.TemplateContent);
        Assert.Equal("Oude inhoud",(await service.GetMailTemplatesAsync()).Single().TemplateContent);
    }
}
