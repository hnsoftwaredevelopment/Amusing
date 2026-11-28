using Amusing.Models;
using Amusing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Amusing.Components.Pages;

public partial class MaintenanceSubscriptions
{
    [Inject] protected WaitlistService WaitlistColumn {get;set;}=default!;
    [Inject] protected AuthenticationStateProvider WaitlistAuthentication {get;set;}=default!;
    [Inject] protected ILogger<MaintenanceSubscriptions> WaitlistLogger {get;set;}=default!;
    private bool waitlistBusy;
    private RegistrationModel? pendingWaitlistPlacement;
    private DateOnly? waitlistDue;

    private async Task ToggleWaitlistAsync(RegistrationModel row)
    {
        if(waitlistBusy || pendingWaitlistPlacement is not null)return;
        waitlistBusy=true;
        try
        {
            if(row.Waitlisted)
            {
                var overview=await WaitlistColumn.ReadAsync(row.FestivalId);
                if(overview is null){await ToastService.ShowErrorAsync("De festivaleditie bestaat niet.");return;}
                if(WaitlistColumn.Today>DateOnly.FromDateTime(overview.Closing))
                {
                    pendingWaitlistPlacement=row;waitlistDue=null;return;
                }
                await PlaceWaitlistRowAsync(row,null);
            }
            else
            {
                var user=(await WaitlistAuthentication.GetAuthenticationStateAsync()).User;
                var error=await WaitlistColumn.ReturnToWaitlistAsync(user,row.FestivalId,row.GroepId);
                if(error is not null)await ToastService.ShowErrorAsync(error);
                else await ToastService.ShowSuccessAsync("Het koor staat nu op de wachtlijst.");
            }
            await LoadRegistrationsAsync();
        }
        catch(Exception error)
        {
            WaitlistLogger.LogError(error,"Wachtlijststatus wijzigen mislukt voor festival {Festival} en koor {Group}",row.FestivalId,row.GroepId);
            await ToastService.ShowErrorAsync("De wachtlijststatus kon niet worden gewijzigd. Ververs de lijst en probeer opnieuw.");
        }
        finally{waitlistBusy=false;}
    }
    private async Task PlaceWaitlistRowAsync(RegistrationModel row,DateOnly? due)
    {
        var user=(await WaitlistAuthentication.GetAuthenticationStateAsync()).User;
        var result=await WaitlistColumn.PlaceAsync(user,row.FestivalId,[row.GroepId],due);
        if(result.Error is not null)await ToastService.ShowErrorAsync(result.Error);
        else if(result.Placed==0)await ToastService.ShowErrorAsync("De inschrijving is inmiddels gewijzigd. De lijst wordt ververst.");
        else
        {
            pendingWaitlistPlacement=null;
            await ToastService.ShowSuccessAsync("Het koor is geplaatst. Bevestiging, nota en organisatiebericht worden klaargezet voor verzending.");
        }
    }
    private async Task ConfirmWaitlistPlacementAsync()
    {
        if(waitlistBusy || pendingWaitlistPlacement is not {} row)return;
        waitlistBusy=true;
        try{await PlaceWaitlistRowAsync(row,waitlistDue);await LoadRegistrationsAsync();}
        catch(Exception error)
        {
            WaitlistLogger.LogError(error,"Plaatsing via inschrijvingen mislukt");
            await ToastService.ShowErrorAsync("De plaatsing kon niet worden opgeslagen. Probeer opnieuw.");
        }
        finally{waitlistBusy=false;}
    }
}
