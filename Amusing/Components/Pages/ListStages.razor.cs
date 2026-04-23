using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class ListStages : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected StageService StageService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDoneActive = false;
    protected bool _initialLoadDoneInactive = false;
    protected bool CanExportActive => VisibleRowCountActive > 0;
    protected bool CanExportInactive => VisibleRowCountInactive > 0;
    protected SfGrid<StageModel> GridRefActive;
    protected SfGrid<StageModel> GridRefInactive;
    protected int VisibleRowCountActive = 0;
    protected int VisibleRowCountInactive = 0;
    protected List<StageModel> ActievePodia = [];
    protected List<StageModel> InactievePodia = [];
    protected string FileNameActive = "Actieve podia";
    protected string FileNameInactive = "Inactieve podia";

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        ActievePodia = await StageService.GetActiveStagesAsync();
        InactievePodia = await StageService.GetInActiveStagesAsync();
        IsLoading = false;
    }

    // Manage direct search functionality
    public async void OnInputActive(InputEventArgs args)
    {
        await GridRefActive.SearchAsync(args.Value);

        await Task.Delay(50);
        await UpdateVisibleRowCountActiveAsync();
    }

    public async void OnInputInactive(InputEventArgs args)
    {
        await GridRefInactive.SearchAsync(args.Value);

        await Task.Delay(50);
        await UpdateVisibleRowCountInactiveAsync();
    }

    protected async Task UpdateVisibleRowCountActiveAsync()
    {
        if (GridRefActive is not null)
        {
            await GridRefActive.Refresh();
            await Task.Delay(50);
            var records = await GridRefActive.GetCurrentViewRecordsAsync();
            await Task.Delay(150);
            VisibleRowCountActive = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    protected async Task UpdateVisibleRowCountInactiveAsync()
    {
        if (GridRefInactive is not null)
        {
            await GridRefInactive.Refresh();
            await Task.Delay(50);
            var records = await GridRefInactive.GetCurrentViewRecordsAsync();
            await Task.Delay(150);
            VisibleRowCountInactive = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    protected async Task OnGridDataBoundActive()
    {
        if (!_initialLoadDoneActive)
        {
            _initialLoadDoneActive = true;
            await UpdateVisibleRowCountActiveAsync();
        }
    }

    protected async Task OnGridDataBoundInactive()
    {
        if (!_initialLoadDoneInactive)
        {
            _initialLoadDoneInactive = true;
            await UpdateVisibleRowCountInactiveAsync();
        }
    }

    // Export functions
    protected async Task ExportToExcelActive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameActive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await GridRefActive!.ExportToExcelAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Lijsten", "Podia", "success", _report);
    }

    protected async Task ExportToExcelInactive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameInactive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await GridRefInactive!.ExportToExcelAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Lijsten", "Podia", "success", _report);
    }

    protected async Task ExportToCsvActive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameActive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await GridRefActive!.ExportToCsvAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Lijsten", "Podia", "success", _report);
    }

    protected async Task ExportToCsvInactive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameInactive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await GridRefInactive!.ExportToCsvAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Lijsten", "Podia", "success", _report);
    }

    protected async Task ExportToPdfActive()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"{FileNameActive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize = PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await GridRefActive!.ExportToPdfAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Lijsten", "Podia", "success", _report);
    }

    protected async Task ExportToPdfInactive()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"{FileNameInactive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize = PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await GridRefInactive!.ExportToPdfAsync(exportProps);

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync("Lijsten", "Podia", "success", _report);
    }

    // Ensure grid-dependent operations run after first render
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        // If the grid reference exists, update counts and refresh it
        if (GridRefActive != null)
        {
            // Update visible row count safely
            await UpdateVisibleRowCountActiveAsync();

            // Refresh the grid to ensure it renders the bound datasource
            try
            {
                await GridRefActive.Refresh();
            }
            catch
            {
                // Swallow any refresh exceptions to avoid breaking first render
                // (grid refresh is best-effort here)
            }
        }
        else
        {
            // Ensure UI is updated if grid ref is not available
            StateHasChanged();
        }

        if (GridRefInactive != null)
        {
            // Update visible row count safely
            await UpdateVisibleRowCountInactiveAsync();

            // Refresh the grid to ensure it renders the bound datasource
            try
            {
                await GridRefInactive.Refresh();
            }
            catch
            {
                // Swallow any refresh exceptions to avoid breaking first render
                // (grid refresh is best-effort here)
            }
        }
        else
        {
            // Ensure UI is updated if grid ref is not available
            StateHasChanged();
        }
    }

    protected async Task UpdateVisibleRowCountStage()
    {
        if (GridRefActive == null)
        {
            VisibleRowCountActive = 0;
            return;
        }

        var data = await GridRefActive.GetCurrentViewRecordsAsync();
        VisibleRowCountActive = data?.Count ?? 0;
    }
}