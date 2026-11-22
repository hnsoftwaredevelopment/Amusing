using System.Dynamic;
using System.Text.RegularExpressions;
using Amusing.Models;
using MimeKit;

namespace Amusing.Services.MailQueue;

public sealed record BulkMailDraft(Guid Id, uint TemplateId, string TemplateName, IReadOnlyList<QueueMessage> Messages)
{
    public static BulkMailDraft Create(TemplatesListModel template, IReadOnlyList<ExpandoObject> recipients, EmailSettings settings)
    {
        var id=Guid.NewGuid();var messages=new List<QueueMessage>();var mapping=new FieldMappingService();
        string subject=mapping.ReplaceLabelsWithKeys(template.TemplateSubject??"");
        string body=mapping.ReplaceLabelsWithKeys(template.TemplateContent??"");
        foreach(IDictionary<string,object?> row in recipients)
        {
            string address=row.TryGetValue("Email",out var email)?email?.ToString()?.Trim()??"":"";
            if(address.Length>320 || !MailboxAddress.TryParse(address,out var mailbox))
            {
                messages.Add(new(address[..Math.Min(address.Length,320)],[],"Ongeldig of ontbrekend e-mailadres."));continue;
            }
            string Personalize(string text) => Regex.Replace(text,@"\{([^{}]+)\}",match=>
                row.FirstOrDefault(kv=>string.Equals(kv.Key,match.Groups[1].Value,StringComparison.OrdinalIgnoreCase)).Value?.ToString()??match.Value);
            var mime=new MimeMessage();mime.From.Add(new MailboxAddress(settings.SenderName,settings.SenderAddress));mime.To.Add(mailbox);
            mime.MessageId=$"{id:N}.{messages.Count}@amusing-hengelo.nl";mime.Subject=Personalize(subject);
            mime.Body=new TextPart("html"){Text=Personalize(body)};
            using var stream=new MemoryStream();mime.WriteTo(stream);messages.Add(new(mailbox.Address,stream.ToArray()));
        }
        return new(id,template.TemplateId,template.TemplateName??"Onbekend sjabloon",messages);
    }
}

