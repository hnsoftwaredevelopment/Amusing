using Amusing.Models;
using Amusing.Services;
using Microsoft.AspNetCore.Components;

namespace Amusing.Components.Pages;

public partial class MaintenanceGroups
{
    [Inject] private ParticipationInvoiceService InvoiceService { get; set; } = default!;
    [Inject] private ILogger<MaintenanceGroups> InvoiceLogger { get; set; } = default!;
    private ParticipationInvoice? InvoicePreview;
    private bool InvoiceBusy;
    private string? InvoiceMessage;
    private bool InvoiceDialogVisible
    {
        get => InvoicePreview is not null;
        set { if (!value && !InvoiceBusy) InvoicePreview = null; }
    }

    private async Task PreviewInvoiceAsync(GroupRegistrationModel registration)
    {
        if (InvoiceBusy || SelectedGroup is null) return;
        var groupId = SelectedGroup.GroupId;
        InvoiceBusy = true;
        InvoicePreview = null;
        InvoiceMessage = null;
        try
        {
            var preview = await InvoiceService.PreviewAsync(groupId, registration.FestivalId);
            if (SelectedGroup?.GroupId == groupId) InvoicePreview = preview;
        }
        catch (InvalidOperationException ex) { InvoiceMessage = ex.Message; }
        catch (Exception ex)
        {
            InvoiceLogger.LogError(ex, "Could not prepare invoice for group {GroupId}", groupId);
            InvoiceMessage = "De factuur kon niet worden opgehaald. Probeer het later opnieuw.";
        }
        finally { InvoiceBusy = false; }
    }

    private async Task SendInvoiceAsync()
    {
        var preview = InvoicePreview;
        if (InvoiceBusy || preview is null || SelectedGroup?.GroupId != preview.GroupId) return;
        InvoiceBusy = true;
        InvoiceMessage = null;
        try
        {
            var error = await InvoiceService.SendAsync(preview);
            if (error is not null)
            {
                InvoiceMessage = "De mailserver heeft de verzending niet bevestigd. Controleer het logboek voordat je opnieuw verstuurt.";
                return;
            }
            InvoicePreview = null;
            InvoiceMessage = $"De kopie van de factuur voor {preview.GroupName} ({preview.Year}) is verstuurd naar {preview.RecipientEmail}.";
            await ToastService.ShowSuccessAsync(preview.SuccessMessage, "Factuur verzonden");
            try
            {
                await LoggingService.WriteUserActionGroupAsync(preview.GroupId, "Beheer", "Factuur opnieuw verzenden", "success",
                    $"<_userName> heeft de factuur voor {preview.GroupName}, festival {preview.Year}, opnieuw verzonden naar {preview.RecipientEmail}. Totaal {ParticipationInvoice.Money(preview.Total)}.");
            }
            catch (Exception ex)
            {
                InvoiceLogger.LogError(ex, "Invoice sent but group audit failed for {GroupId}", preview.GroupId);
                InvoiceMessage += " De groepslog kon niet worden bijgewerkt; verstuur de factuur niet opnieuw.";
            }
        }
        catch (InvalidOperationException ex)
        {
            InvoicePreview = null;
            InvoiceMessage = ex.Message;
        }
        catch (Exception ex)
        {
            InvoiceLogger.LogError(ex, "Invoice send did not complete for {GroupId}", preview.GroupId);
            InvoicePreview = null;
            InvoiceMessage = "De verzending kon niet worden bevestigd. Controleer het logboek voordat je opnieuw verstuurt.";
        }
        finally { InvoiceBusy = false; }
    }
}
