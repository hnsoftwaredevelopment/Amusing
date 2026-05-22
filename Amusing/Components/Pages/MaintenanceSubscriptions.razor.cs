using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class MaintenanceSubscriptions : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] protected RegistrationService RegistrationService { get; set; } = default!;
    [Inject] protected StageTypeService StageTypeService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;

    protected SfGrid<RegistrationModel> GridRef;
    protected List<Edition> Editions = [];
    protected List<RegistrationModel> RegistrationList = [];
    protected List<AvailableGroupModel> AvailableGroups = [];
    protected List<StageTypeModel> AvailableStageTypes = [];
    protected string? SelectedEditionId;
    protected int VisibleRowCount = 0;
    private bool showPaymentDialog;
    private bool showAddGroupDialog;
    private uint? selectedAvailableGroupId;
    private int? selectedAantalDeelnemers;
    private string? selectedPodiumsoort;

    private readonly List<string> YesNoList = ["Ja", "Nee"];

    private bool showDatePicker { get; set; }
    private DateTime? selectedDate { get; set; }
    private RegistrationModel? selectedRegistration;

    protected string SelectedEditionText => Editions.FirstOrDefault(e => e.ID == SelectedEditionId)?.Text ?? "Onbekende editie";
    protected bool IsCurrentEditionSelected => Editions.FirstOrDefault()?.ID == SelectedEditionId;
    protected bool CanAddGroup => IsCurrentEditionSelected && AvailableGroups.Count > 0;
    protected bool CanConfirmAddGroup =>
        CanAddGroup &&
        selectedAvailableGroupId is not null &&
        selectedAantalDeelnemers is > 0 &&
        !string.IsNullOrWhiteSpace(selectedPodiumsoort);

    protected override async Task OnInitializedAsync()
    {
        Editions = await EditionService.GetEditionsAsync();
        AvailableStageTypes = await StageTypeService.GetActiveStageTypesListAsync();

        if (Editions.Any())
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending(e => int.Parse(e.Text))
                .First().ID;

            // Get the registrations for the selected edition
            RegistrationList = await RegistrationService.GetRegistrationsByFestivalIdAsync(Convert.ToUInt32(SelectedEditionId));
            await LoadAvailableGroupsAsync();
            if (SelectedEditionId != null && RegistrationList.Count > 0)
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

    protected async Task OnEditionChanged(string selectedId)
    {
        if (string.IsNullOrWhiteSpace(selectedId))
            return;

        SelectedEditionId = selectedId;

        await LoadRegistrationsAsync();
        await LoadAvailableGroupsAsync();

        if (GridRef != null)
        {
            await GridRef.Refresh();
            VisibleRowCount = RegistrationList.Count;
        }
    }

    protected async Task LoadRegistrationsAsync()
    {
        if (Convert.ToUInt32(SelectedEditionId) != 0)
        {
            RegistrationList = await RegistrationService.GetRegistrationsByFestivalIdAsync(Convert.ToUInt32(SelectedEditionId));
        }
    }

    protected async Task LoadAvailableGroupsAsync()
    {
        if (!IsCurrentEditionSelected || string.IsNullOrWhiteSpace(SelectedEditionId))
        {
            AvailableGroups = [];
            selectedAvailableGroupId = null;
            return;
        }

        AvailableGroups = await RegistrationService.GetNotRegisteredGroupsAsync(Convert.ToUInt32(SelectedEditionId));
        selectedAvailableGroupId = AvailableGroups.FirstOrDefault()?.ZanggroepId;
    }

    // Manage direct search functionality
    public void OnInput(InputEventArgs args)
    {
        this.GridRef.SearchAsync(args.Value);

        // Count the Number of visible rows
        _ = Task.Run(async () =>
        {
            await Task.Delay(200); // Short delay to handle fast typers
            await InvokeAsync(UpdateVisibleRowCount);
        });
    }

    protected async Task UpdateVisibleRowCount()
    {
        if (GridRef == null)
            return;

        var data = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = data?.Count ?? 0;
    }

    // Open dialog for a specific registration
    private void OpenPaymentDialog(RegistrationModel registration)
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
        if (selectedRegistration != null)
        {
            await RegistrationService.UpdatePaymentStatusAsync(
                selectedRegistration.FestivalId,
                selectedRegistration.GroepId,
                selectedDate
            );

            // Refresh local UI model
            selectedRegistration.Betaald = "Ja";
            await ToastService.ShowSuccessAsync( $"Betaling voor {selectedRegistration.Naam} is opgeslagen." );
        }

        ClosePaymentDialog();
        StateHasChanged();
    }

    // Handle "Gestorneerd" button
    private async Task TogglePaymentAsync(RegistrationModel registration)
    {
        await RegistrationService.UpdatePaymentStatusAsync(
            registration.FestivalId,
            registration.GroepId,
            null
        );

        registration.Betaald = "Nee";
        await ToastService.ShowSuccessAsync( $"Betaling voor {registration.Naam} is teruggezet naar niet betaald." );
        StateHasChanged();
    }

    private async Task ToggleDropOutAsync(RegistrationModel registration)
    {
        DateOnly? newValue;

        if (registration.Afgehaakt == "Nee")
        {
            // Koort haakt af → zet huidige datum
            newValue = DateOnly.FromDateTime(DateTime.Now);
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
        registration.Afgehaakt = (newValue == null) ? "Nee" : "Ja";
        await ToastService.ShowSuccessAsync( $"Afhaakstatus voor {registration.Naam} is opgeslagen." );
        StateHasChanged();
    }

    private async Task OnYesNoChangedAsync(
    RegistrationModel registration,
    string columnName,
    string value)
    {
        // Update UI model
        typeof(RegistrationModel)
            .GetProperty(columnName)!
            .SetValue(registration, value);

        // Update DB
        await RegistrationService.UpdateYesNoFieldAsync(
            registration.FestivalId,
            registration.GroepId,
            columnName,
            value
        );
        await ToastService.ShowSuccessAsync( $"De wijziging voor {registration.Naam} is opgeslagen." );
    }

    private bool showYesNoDialog;
    private string? selectedYesNoField;
    private string? selectedYesNoHeaderText;
    private static class YesNoHeaders
    {
        public const string Bevestigd = "Bevestiging wijzigen";
        public const string Kleedkamer = "Gebruik kleedkamer wijzigen";
        public const string SingAlong = "Deelname sing along wijzigen";
        public const string AcapellaBattle = "Deelname acapella battle wijzigen";
        public const string Beoordeling = "Wil beoordeling wijzigen";
    }
    private void OpenYesNoDialog(RegistrationModel registration, string fieldName, string headerText)
    {
        selectedRegistration = registration;
        selectedYesNoField = fieldName;
        selectedYesNoHeaderText = headerText;

        showYesNoDialog = true;
    }

    private async Task ConfirmYesNoAsync(string value)
    {
        if (selectedRegistration is null || string.IsNullOrEmpty(selectedYesNoField))
            return;

        string mappedFieldName = selectedYesNoField switch
        {
            nameof(RegistrationModel.Bevestigd) => "Bevestigd",
            nameof(RegistrationModel.Kleedkamer) => "wens_1",
            nameof(RegistrationModel.SingAlong) => "wens_2",
            nameof(RegistrationModel.AcapellaBattle) => "wens_3",
            nameof(RegistrationModel.Beoordeling) => "wens_4",
            _ => throw new InvalidOperationException("Unknown Yes/No field")
        };

        // Update model
        typeof(RegistrationModel)
            .GetProperty(selectedYesNoField)!
            .SetValue(selectedRegistration, value);

        await RegistrationService.UpdateYesNoFieldAsync(
            selectedRegistration.FestivalId,
            selectedRegistration.GroepId,
            mappedFieldName,
            value
        );

        showYesNoDialog = false;
        await ToastService.ShowSuccessAsync( $"De wijziging voor {selectedRegistration.Naam} is opgeslagen." );
    }


    private Task OnYesClicked()
    {
        return ConfirmYesNoAsync("Ja");
    }

    private Task OnNoClicked()
    {
        return ConfirmYesNoAsync("Nee");
    }

    private void CloseYesNoDialog()
    {
        showYesNoDialog = false;
        selectedRegistration = null;
        selectedYesNoField = null;
        selectedYesNoHeaderText = null;
    }

    private void OpenAddGroupDialog()
    {
        if (!CanAddGroup)
            return;

        selectedAvailableGroupId = AvailableGroups.FirstOrDefault()?.ZanggroepId;
        selectedAantalDeelnemers = 20;
        selectedPodiumsoort = AvailableStageTypes.FirstOrDefault()?.Type;
        showAddGroupDialog = true;
    }

    private void CloseAddGroupDialog()
    {
        showAddGroupDialog = false;
        selectedAvailableGroupId = null;
        selectedAantalDeelnemers = null;
        selectedPodiumsoort = null;
    }

    private async Task ConfirmAddGroupAsync()
    {
        if (!CanConfirmAddGroup || string.IsNullOrWhiteSpace(SelectedEditionId))
            return;

        uint groupId = selectedAvailableGroupId!.Value;
        string groupName = AvailableGroups.FirstOrDefault(group => group.ZanggroepId == groupId)?.Naam ?? "het koor";
        await RegistrationService.AddRegistrationAsync(
            Convert.ToUInt32(SelectedEditionId),
            groupId,
            selectedAantalDeelnemers!.Value,
            selectedPodiumsoort!);
        await LoadRegistrationsAsync();
        await LoadAvailableGroupsAsync();

        if (GridRef != null)
        {
            await GridRef.Refresh();
        }

        VisibleRowCount = RegistrationList.Count;
        CloseAddGroupDialog();
        await ToastService.ShowSuccessAsync( $"{groupName} is ingeschreven voor editie {SelectedEditionText}." );
        StateHasChanged();
    }
}
