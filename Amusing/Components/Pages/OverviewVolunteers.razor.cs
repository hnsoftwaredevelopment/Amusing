using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class OverviewVolunteers : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] protected VolunteerService VolunteerService { get; set; } = default!;

    protected SfGrid<VolunteerModel> GridRef;
    protected SfGrid<VolunteerModel> StageGridRef;
    protected SfGrid<VolunteerModel> OtherGridRef;
    protected SfGrid<VolunteerModel> DroppedOutGridRef;
    protected List<Edition> Editions = new();
    protected List<VolunteerModel> AllVolunteerList = new();
    protected List<VolunteerModel> FilteredStageVolunteerList = new();
    protected List<VolunteerModel> FilteredOtherVolunteerList = new();
    protected List<VolunteerModel> FilteredDroppedOutVolunteerList = new();
    protected string? SelectedEditionId;
    protected bool IsInitialized; // used to be sure all Refs are initialized
    protected int VisibleRowCount = 0;
    protected int VisibleRowCountStage = 0;
    protected int VisibleRowCountOther = 0;
    protected int VisibleRowCountDroppedOut = 0;

    protected string SelectedEditionText => Editions.FirstOrDefault(e => e.ID == SelectedEditionId)?.Text ?? "Onbekende editie";


    // Editions list
    protected override async Task OnInitializedAsync()
    {
        Editions = await EditionService.GetEditionsAsync();

        if (Editions.Any())
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending(e => int.Parse(e.Text))
                .First().ID;

            // Get the volunteers for the selected edition
            AllVolunteerList = await VolunteerService.GetVolunteersByFestivalIdAsync(Convert.ToUInt32(SelectedEditionId));
            ApplyVolunteerFilters();
            if (SelectedEditionId != null && AllVolunteerList.Count > 0)
            {
                await UpdateVisibleRowCountStage();
                await UpdateVisibleRowCountOther();
                await UpdateVisibleRowCountDroppedOut();
            }
        }
    }

    // Whenever the selected edition changes the datagrid has to be updated
    protected async Task OnEditionChanged(string selectedId)
    {
        if (string.IsNullOrWhiteSpace(selectedId))
            return;

        SelectedEditionId = selectedId;
        await LoadVolunteersAsync();
        ApplyVolunteerFilters();
        StateHasChanged(); // Force UI rerender to initializeall Refs

        if (IsInitialized)
        {
            await StageGridRef.Refresh();
            await OtherGridRef.Refresh();
            await DroppedOutGridRef.Refresh();

            VisibleRowCountStage = FilteredStageVolunteerList.Count;
            VisibleRowCountOther = FilteredOtherVolunteerList.Count;
            VisibleRowCountDroppedOut = FilteredDroppedOutVolunteerList.Count;
        }
    }

    // Volunteers
    protected async Task LoadVolunteersAsync()
    {
        if (Convert.ToUInt32(SelectedEditionId) != 0)
        {
            AllVolunteerList = await VolunteerService.GetVolunteersByFestivalIdAsync(Convert.ToUInt32(SelectedEditionId));

            VisibleRowCount = AllVolunteerList.Count;
        }
    }

    // Search
    public void OnInputStage(InputEventArgs args)
    {
        this.StageGridRef.SearchAsync(args.Value);

        // Count the Number of visible rows
        _ = Task.Run(async () =>
        {
            await Task.Delay(200); // Short delay to handle fast typers
            await InvokeAsync(UpdateVisibleRowCountStage);
        });
    }

    public void OnInputOther(InputEventArgs args)
    {
        this.OtherGridRef.SearchAsync(args.Value);

        // Count the Number of visible rows
        _ = Task.Run(async () =>
        {
            await Task.Delay(200); // Short delay to handle fast typers
            await InvokeAsync(UpdateVisibleRowCountOther);
        });
    }

    public void OnInputDroppedOut(InputEventArgs args)
    {
        this.DroppedOutGridRef.SearchAsync(args.Value);

        // Count the Number of visible rows
        _ = Task.Run(async () =>
        {
            await Task.Delay(200); // Short delay to handle fast typers
            await InvokeAsync(UpdateVisibleRowCountDroppedOut);
        });
    }

    // Make sure all Refs are initialized
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (SelectedEditionId != null && AllVolunteerList.Count > 0)
            {
                await UpdateVisibleRowCountStage();
                await UpdateVisibleRowCountOther();
                await UpdateVisibleRowCountDroppedOut();
            }
        }
    }

    // Apply filtering for the Datasurces of the DataGrids
    protected void ApplyVolunteerFilters()
    {
        FilteredStageVolunteerList = AllVolunteerList.Where(v => v.Afgehaakt == "nee" && v.Podiumdienst == "ja").ToList();
        FilteredOtherVolunteerList = AllVolunteerList.Where(v => v.Afgehaakt == "nee" && v.Podiumdienst == "nee").ToList();
        FilteredDroppedOutVolunteerList = AllVolunteerList.Where(v => v.Afgehaakt == "ja").ToList();

        VisibleRowCountStage = FilteredStageVolunteerList.Count;
        VisibleRowCountOther = FilteredOtherVolunteerList.Count;
        VisibleRowCountDroppedOut = FilteredDroppedOutVolunteerList.Count;
    }

    // Row Counts
    protected async Task UpdateVisibleRowCountStage()
    {
        if (StageGridRef == null)
        {
            VisibleRowCountStage = 0;
            return;
        }

        var data = await StageGridRef.GetCurrentViewRecordsAsync();
        VisibleRowCountStage = data?.Count ?? 0;
    }

    protected async Task UpdateVisibleRowCountOther()
    {
        if (OtherGridRef == null)
        {
            VisibleRowCountOther = 0;
            return;
        }

        var data = await OtherGridRef.GetCurrentViewRecordsAsync();
        VisibleRowCountOther = data?.Count ?? 0;
    }

    protected async Task UpdateVisibleRowCountDroppedOut()
    {
        if (DroppedOutGridRef == null)
        {
            VisibleRowCountDroppedOut = 0;
            return;
        }

        var data = await DroppedOutGridRef.GetCurrentViewRecordsAsync();
        VisibleRowCountDroppedOut = data?.Count ?? 0;
    }

    protected async Task ExportToExcel()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"Vrijwilligers {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await GridRef!.ExportToExcelAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Overzichten", "Vrijwilligers", "success", _report);
    }

    protected async Task ExportToCsv()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"Vrijwilligers {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await GridRef!.ExportToCsvAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Overzichten", "Vrijwilligers", "success", _report);
    }

    protected async Task ExportToPdf()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"Vrijwilligers {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize = PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await GridRef!.ExportToPdfAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Overzichten", "Vrijwilligers", "success", _report);
    }
}