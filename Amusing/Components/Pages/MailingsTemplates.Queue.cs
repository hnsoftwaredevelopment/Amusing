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
    private readonly BulkMailSubmission _queueSubmission=new();
    private string? _queuedCampaignId=>_queueSubmission.CampaignId;
    private bool _isDeleting;
    private bool QueueInputsLocked=>_isDeleting||_isSendingMail||_queueSubmission.IsPending;
    private bool QueueEnabled => Configuration.GetValue<bool>("MailQueue:Enabled");
    private void ResetQueueRequest(){if(!QueueInputsLocked)_queueSubmission.Reset();}
    private async Task QueueCampaignAsync(TemplatesListModel template,List<ExpandoObject> recipients)
    {
        if(_queuedCampaignId is not null)
        {await ToastService.ShowAsync("Deze mailing staat al in de wachtrij. Bekijk de voortgang in het wachtrijoverzicht.");return;}
        var user=(await QueueAuthentication.GetAuthenticationStateAsync()).User;
        if(!uint.TryParse(user.FindFirst("UserId")?.Value,out var actor)||actor==0)throw new InvalidOperationException("Beheeraccount ontbreekt.");
        var draft=_queueSubmission.IsPending?null:MailingService.PrepareQueuedBulkMail(template,recipients);
        if(draft is {Messages.Count:0})
        {await ToastService.ShowWarningAsync("Deze ontvangerslijst bevat geen ontvangers. Kies een andere lijst.");return;}
        await _queueSubmission.SubmitAsync(draft,actor,(snapshot,owner)=>
            MailQueue.EnqueueAsync(snapshot.Id,snapshot.TemplateId,snapshot.TemplateName,owner,snapshot.Messages));
        var submitted=_queueSubmission.Draft!;
        int invalid=submitted.Messages.Count(m=>m.ValidationError is not null);
        try
        {
            await LoggingService.WriteUserActionTemplateAsync(submitted.TemplateId,"Mailing","Wachtrij","success",
                $"<_userName> heeft mailing {submitted.TemplateName} in de wachtrij gezet. Campagne: {_queuedCampaignId}. Totaal: {submitted.Messages.Count}, wachtend: {submitted.Messages.Count-invalid}, ongeldig: {invalid}. Nog niet als verzonden bevestigd.");
        }
        catch
        {
            await ToastService.ShowWarningAsync("Mailing staat in de wachtrij, maar de startmelding kon niet worden opgeslagen. Verstuur deze mailing niet opnieuw; bekijk de verzendwachtrij.");
            return;
        }
        await ToastService.ShowSuccessAsync("Mailing staat in de wachtrij. Je kunt deze pagina sluiten; bekijk de voortgang in het wachtrijoverzicht.");
    }
}
