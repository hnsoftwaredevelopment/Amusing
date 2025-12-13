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
    protected List<Edition> Editions = [];
    protected List<RegistrationModel> RegistrationList = new();
    protected string? SelectedEditionId;
    protected int VisibleRowCount = 0;
    private bool showPaymentDialog;

    private bool showDatePicker { get; set; }
    private DateTime? selectedDate { get; set; }
    private RegistrationModel? selectedRegistration;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Editions?.Count > 0)
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
        if (GridRef == null)
            return;

        var data = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = data?.Count ?? 0;
    }

    // Open dialog for a specific registration
    private void OpenPaymentDialog( RegistrationModel registration )
    {
        selectedRegistration = registration;
        selectedDate = DateTime.Now; // Default to today
        showPaymentDialog = true;
    }

    // Close dialog without saving
    private void ClosePaymentDialog()
    {
        showPaymentDialog = false;
        selectedRegistration = null;
        selectedDate = null;
    }

    // Confirm payment and update in DB
    private async Task ConfirmPaymentDateAsync()
    {
        if ( selectedRegistration != null )
        {
            await RegistrationService.UpdatePaymentStatusAsync(
                selectedRegistration.FestivalId,
                selectedRegistration.GroepId,
                selectedDate
            );

            // Refresh local UI model
            selectedRegistration.Betaald = "Ja";
        }

        ClosePaymentDialog();
        StateHasChanged();
    }

    // Handle "Gestorneerd" button
    private async Task TogglePaymentAsync( RegistrationModel registration )
    {
        await RegistrationService.UpdatePaymentStatusAsync(
            registration.FestivalId,
            registration.GroepId,
            null
        );

        registration.Betaald = "Nee";
        StateHasChanged();
    }

    private async Task ToggleDropOutAsync( RegistrationModel registration )
    {
        DateOnly? newValue;

        if ( registration.Afgehaakt == "Nee" )
        {
            // Koort haakt af → zet huidige datum
            newValue = DateOnly.FromDateTime( DateTime.Now );
        }
        else
        {
            // Koort haakt aan → zet NULL
            newValue = null;
        }

        // Update in DB
        await RegistrationService.UpdateDropOutStatusAsync(
            registration.FestivalId,
            registration.GroepId,
            newValue
        );

        // Update UI-model
        registration.Afgehaakt = ( newValue == null ) ? "Nee" : "Ja";
        StateHasChanged();
    }
}