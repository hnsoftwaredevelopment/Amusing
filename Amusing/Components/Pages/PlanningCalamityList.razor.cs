using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class PlanningCalamityList
{
    [Inject] public PlanningService PlanningService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    protected List<Edition> Editions { get; set; } = [];
    protected string? SelectedEditionId { get; set; }
    protected string SelectedEditionText { get; set; } = string.Empty;
    protected List<PlanningCalamityListRow> Rows { get; set; } = [];
    protected SfGrid<PlanningCalamityListRow> GridRef = default!;
    protected int VisibleRowCount;

    private const string FileName = "Calamiteitenlijst";

    protected override async Task OnInitializedAsync()
    {
        Editions = await EditionService.GetEditionsAsync();

        if (Editions.Count == 0)
            return;

        SelectedEditionId = Editions
            .OrderByDescending(e => int.Parse(e.Text))
            .First().ID;

        SetSelectedEditionText();
        await LoadRowsAsync();
    }

    protected async Task OnEditionChanged(string selectedId)
    {
        if (string.IsNullOrWhiteSpace(selectedId))
            return;

        SelectedEditionId = selectedId;
        SetSelectedEditionText();
        await LoadRowsAsync();
    }

    protected async Task OnInput(InputEventArgs args)
    {
        await GridRef.SearchAsync(args.Value);
        await Task.Delay(50);
        await UpdateVisibleRowCountAsync();
    }

    protected async Task ExportToExcel()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = BuildFileName("xlsx")
        };

        await ToastService.ShowExportStartedAsync(exportProps.FileName);
        await GridRef.ExportToExcelAsync(exportProps);
        await ToastService.ShowExportCompletedAsync(exportProps.FileName, "Excel");
        await LogExportAsync(exportProps.FileName);
    }

    protected async Task ExportToPdf()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = BuildFileName("pdf"),
            PageOrientation = PageOrientation.Landscape,
            PageSize = PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };

        await ToastService.ShowExportStartedAsync(exportProps.FileName);
        await GridRef.ExportToPdfAsync(exportProps);
        await ToastService.ShowExportCompletedAsync(exportProps.FileName, "PDF");
        await LogExportAsync(exportProps.FileName);
    }

    protected async Task ExportToWord()
    {
        if (!int.TryParse(SelectedEditionId, out var festivalId))
            return;

        var fileName = BuildFileName("docx");

        await ToastService.ShowExportStartedAsync(fileName);
        var bytes = await PlanningService.ExportCalamityListToWordAsync(festivalId, $"{FileName} {SelectedEditionText}");

        await JS.InvokeVoidAsync(
            "downloadFile",
            fileName,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            Convert.ToBase64String(bytes));

        await ToastService.ShowExportCompletedAsync(fileName, "Word");
        await LogExportAsync(fileName);
    }

    protected async Task Print()
    {
        await GridRef.PrintAsync();
        await LoggingService.WriteUserActionAsync("Planning", FileName, "success", "<_userName> heeft de calamiteitenlijst afgedrukt");
    }

    protected async Task OnGridDataBound()
    {
        await UpdateVisibleRowCountAsync();
    }

    protected async Task OnGridActionComplete(ActionEventArgs<PlanningCalamityListRow> args)
    {
        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Refresh ||
            args.RequestType == Syncfusion.Blazor.Grids.Action.Filtering ||
            args.RequestType == Syncfusion.Blazor.Grids.Action.Paging ||
            args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting ||
            args.RequestType == Syncfusion.Blazor.Grids.Action.Searching)
        {
            await UpdateVisibleRowCountAsync();
        }
    }

    private async Task LoadRowsAsync()
    {
        if (!int.TryParse(SelectedEditionId, out var festivalId))
            return;

        Rows = await PlanningService.GetPlanningCalamityListAsync(festivalId);

        if (GridRef is not null)
            await GridRef.Refresh();
    }

    private void SetSelectedEditionText()
    {
        SelectedEditionText = Editions.FirstOrDefault(e => e.ID == SelectedEditionId)?.Text ?? string.Empty;
    }

    private static string BuildFileName(string extension)
    {
        return $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.{extension}";
    }

    private async Task LogExportAsync(string fileName)
    {
        string report = $"<_userName> heeft \"{fileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Planning", FileName, "success", report);
    }

    private async Task UpdateVisibleRowCountAsync()
    {
        if (GridRef == null)
            return;

        var records = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = records?.Count ?? 0;
        StateHasChanged();
    }
}
