using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class MaintenanceSubscriptions : ComponentBase
{
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
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
    private bool showAddGroupDialog;
    private uint? selectedAvailableGroupId;
    private int? selectedAantalDeelnemers;
    private string? selectedPodiumsoort;



    protected string SelectedEditionText => Editions.FirstOrDefault(e => e.ID == SelectedEditionId)?.Text ?? "Onbekende editie";
    protected bool IsCurrentEditionSelected => Editions.FirstOrDefault()?.ID == SelectedEditionId;
    protected bool CanAddGroup => IsCurrentEditionSelected && AvailableGroups.Count > 0;
    protected bool CanConfirmAddGroup =>
        CanAddGroup &&
        selectedAvailableGroupId is not null &&
        selectedAantalDeelnemers is > 0 &&
        !string.IsNullOrWhiteSpace(selectedPodiumsoort);

    protected void OnRegistrationDoubleClick(RecordDoubleClickEventArgs<RegistrationModel> args)
    {
        if (args.RowData is { GroepId: > 0 } registration)
        {
            Navigation.NavigateTo($"/Maintenance-Groups?groupId={registration.GroepId}");
        }
    }

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
    public async Task OnInput(InputEventArgs args)
    {
        await GridRef.SearchAsync(args.Value);

    }

    protected async Task UpdateVisibleRowCount()
    {
        if (GridRef == null)
            return;

        var data = await GridRef.GetCurrentViewRecordsAsync();
        var count = data?.Count ?? 0;
        if (VisibleRowCount != count)
        {
            VisibleRowCount = count;
            await InvokeAsync(StateHasChanged);
        }
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

        CloseAddGroupDialog();
        await ToastService.ShowSuccessAsync( $"{groupName} is ingeschreven voor editie {SelectedEditionText}." );
        StateHasChanged();
    }
}
