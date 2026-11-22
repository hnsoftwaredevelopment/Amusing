using System.Dynamic;
using Amusing.Models;
using Amusing.Services.MailQueue;
using MimeKit;
using Xunit;

namespace Beheer.Tests;
public class BulkMailDraftTests
{
    private static ExpandoObject Recipient(string email,string name)
    {IDictionary<string,object?> row=new ExpandoObject();row["Email"]=email;row["Name"]=name;return (ExpandoObject)row;}
    [Fact]public void SnapshotPreservesPersonalizationAndSeparateChoirCopies()
    {
        var template=new TemplatesListModel{TemplateId=22,TemplateName="Invite",TemplateSubject="Hallo {Name}",TemplateContent="<p>{Name}</p>"};
        var draft=BulkMailDraft.Create(template,[Recipient("one@example.invalid","First"),Recipient("one@example.invalid","Second")],new(){SenderAddress="sender@example.invalid",SenderName="Amusing"});
        template.TemplateContent="Changed";
        Assert.Equal(2,draft.Messages.Count);Assert.NotEqual(draft.Messages[0].Mime,draft.Messages[1].Mime);
        using var stream=new MemoryStream(draft.Messages[0].Mime);var mime=MimeMessage.Load(stream);
        Assert.Equal("Hallo First",mime.Subject);Assert.Equal("<p>First</p>",mime.HtmlBody.TrimEnd('\r','\n'));Assert.NotEmpty(mime.MessageId);
    }
    [Fact]public void BadRecipientIsRecordedWithoutBlockingOtherRecipients()
    {
        var draft=BulkMailDraft.Create(new(){TemplateId=22,TemplateName="Test",TemplateSubject="Subject"},
            [Recipient("not an address","Bad"),Recipient("one@example.invalid","Good")],new(){SenderAddress="sender@example.invalid"});
        Assert.NotNull(draft.Messages[0].ValidationError);Assert.Empty(draft.Messages[0].Mime);Assert.Null(draft.Messages[1].ValidationError);
    }
    [Fact]public void DraftUsesOneStableMessageIdPerOriginalRecipientRow()
    {
        var draft=BulkMailDraft.Create(new(){TemplateId=22,TemplateName="Test",TemplateSubject="Subject"},
            [Recipient("one@example.invalid","One"),Recipient("one@example.invalid","Two")],new(){SenderAddress="sender@example.invalid"});
        string Read(int index){using var s=new MemoryStream(draft.Messages[index].Mime);return MimeMessage.Load(s).MessageId!;}
        Assert.NotEqual(Read(0),Read(1));Assert.Equal(Read(0),Read(0));
    }
}
