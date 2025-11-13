using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Calendars;

namespace Amusing.Components.Pages;

public partial class MaintenanceSubscriptions : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] protected RegistrationService RegistrationService { get; set; } = default!;

    protected SfGrid<RegistrationModel> GridRef;
    protected List<Edition> Editions = new();
    protected List<RegistrationModel> RegistrationList = new();
    protected string? SelectedEditionId;
    protected int VisibleRowCount = 0;
    private uint editingPaymentId;
    private bool showDatePicker { get; set; }
    private DateTime? selectedDate { get; set; }

    protected string SelectedEditionText => Editions.FirstOrDefault( e => e.ID == SelectedEditionId )?.Text ?? "Onbekende editie";

    protected override async Task OnInitializedAsync()
    {
        Editions = await EditionService.GetEditionsAsync();

        if ( Editions.Any() )
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending( e => int.Parse( e.Text ) )
                .First().ID;

            // Get the registrations for the selected edition
            RegistrationList = await RegistrationService.GetRegistrationsByFestivalIdAsync( Convert.ToUInt32( SelectedEditionId ) );
            if ( SelectedEditionId != null && RegistrationList.Count > 0 )
            {
                await UpdateVisibleRowCount();
            }
        }
    }

    protected async Task OnEditionChanged( string selectedId )
    {
        if ( string.IsNullOrWhiteSpace( selectedId ) )
            return;

        SelectedEditionId = selectedId;

        await LoadRegistrationsAsync();

        if ( GridRef != null )
        {
            await GridRef.Refresh();
            VisibleRowCount = RegistrationList.Count;
        }
    }

    protected async Task LoadRegistrationsAsync()
    {
        if ( Convert.ToUInt32( SelectedEditionId ) != 0 )
        {
            RegistrationList = await RegistrationService.GetRegistrationsByFestivalIdAsync( Convert.ToUInt32( SelectedEditionId ) );
        }
    }

    // Manage direct search functionality
    public void OnInput( InputEventArgs args )
    {
        this.GridRef.SearchAsync( args.Value );

        // Count the Number of visible rows
        _ = Task.Run( async () =>
        {
            await Task.Delay( 200 ); // Short delay to handle fast typers
            await InvokeAsync( UpdateVisibleRowCount );
        } );
    }

    protected async Task UpdateVisibleRowCount()
    {
        var data = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = data?.Count ?? 0;
    }

    private async Task TogglePaymentAsync( RegistrationModel registration )
    {
        // Als betaald = Ja → Gestorneerd
        if ( registration.Betaald == "Ja" )
        {
            registration.Betaald = "Nee";
            await RegistrationService.UpdatePaymentStatusAsync( registration.FestivalId, registration.GroepId, null );
        }
        else
        {
            // Toon datepicker
            editingPaymentId = registration.FestivalId; // of reg.Id
            selectedDate = DateTime.Now;
        }
    }

    private async Task ConfirmPaymentDateAsync( RegistrationModel registration )
    {
        registration.Betaald = "Ja";
        await RegistrationService.UpdatePaymentStatusAsync( registration.FestivalId, registration.GroepId, selectedDate );
        editingPaymentId = registration.FestivalId;
    }

    private Task CancelPaymentEdit( RegistrationModel registration )
    {
        editingPaymentId = 0;
        showDatePicker = false;
        selectedDate = null;
        return Task.CompletedTask;
    }

    private void OnHasPayedClicked()
    {
        showDatePicker = true;
    }
}