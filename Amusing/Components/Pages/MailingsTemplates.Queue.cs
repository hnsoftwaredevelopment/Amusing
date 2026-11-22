using System.Dynamic;
using Amusing.Models;
using Amusing.Services.MailQueue;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Amusing.Components.Pages;

public partial class MailingsTemplates
{
    [Inject] public MailQueueStore MailQueue {get;set;}=default!;
    [Inject] public AuthenticationStateProvider QueueAuthentication {get;set;}=default!;
    private BulkMailDraft? _queueDraft;
    private string? _queuedCampaignId;
    private bool QueueEnabled => Configuration.GetValue<bool>("MailQueue:Enabled");
    private void ResetQueueRequest(){_queueDraft=null;_queuedCampaignId=null;}
    private async Task QueueCampaignAsync(TemplatesListModel template,List<ExpandoObject> recipients)
    {
        if(_queuedCampaignId is not null)
        {await ToastService.ShowAsync("Deze mailing staat al in de wachtrij. Bekijk de voortgang in het wachtrijoverzicht.");return;}
        var user=(await QueueAuthentication.GetAuthenticationStateAsync()).User;
        if(!uint.TryParse(user.FindFirst("UserId")?.Value,out var actor)||actor==0)throw new InvalidOperationException("Beheeraccount ontbreekt.");
        _queueDraft??=MailingService.PrepareQueuedBulkMail(template,recipients);
        _queuedCampaignId=await MailQueue.EnqueueAsync(_queueDraft.Id,_queueDraft.TemplateId,_queueDraft.TemplateName,actor,_queueDraft.Messages);
        int invalid=_queueDraft.Messages.Count(m=>m.ValidationError is not null);
        await LoggingService.WriteUserActionTemplateAsync(_queueDraft.TemplateId,"Mailing","Wachtrij","success",
            $"<_userName> heeft mailing {_queueDraft.TemplateName} in de wachtrij gezet. Campagne: {_queuedCampaignId}. Totaal: {_queueDraft.Messages.Count}, wachtend: {_queueDraft.Messages.Count-invalid}, ongeldig: {invalid}. Nog niet als verzonden bevestigd.");
        await ToastService.ShowSuccessAsync("Mailing staat in de wachtrij. Je kunt deze pagina sluiten; bekijk de voortgang in het wachtrijoverzicht.");
    }
}
