using System.Collections.Generic;
using System.Threading.Tasks;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class ListTasks : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected TaskService TaskService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDoneActive = false;
    protected bool _initialLoadDoneInactive = false;
    protected SfGrid<TaskModel> GridRefActive;
    protected SfGrid<TaskModel> GridRefInactive;
    protected int VisibleRowCountActive = 0;
    protected int VisibleRowCountInactive = 0;
    protected List<TaskModel> ActieveTaken = [];
    protected List<TaskModel> InactieveTaken = [];
    protected string FileNameActive = "Actieve taken";
    protected string FileNameInactive = "Inactieve taken";

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        ActieveTaken = await TaskService.GetActiveTasksAsync();
        InactieveTaken = await TaskService.GetInActiveTasksAsync();
        IsLoading = false;
    }

    // Manage direct search functionality
    public async void OnInputActive( InputEventArgs args )
    {
        await GridRefActive.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountActiveAsync();
    }

    public async void OnInputInactive( InputEventArgs args )
    {
        await GridRefInactive.SearchAsync( args.Value );
        await Task.Delay( 50 );
        await UpdateVisibleRowCountInactiveAsync();
    }

    protected async Task UpdateVisibleRowCountActiveAsync()
    {
        if ( GridRefActive is not null )
        {
            await GridRefActive.Refresh();
            await Task.Delay( 50 );
            var records = await GridRefActive.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCountActive = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    protected async Task UpdateVisibleRowCountInactiveAsync()
    {
        if ( GridRefInactive is not null )
        {
            await GridRefInactive.Refresh();
            await Task.Delay( 50 );
            var records = await GridRefInactive.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCountInactive = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    protected async Task OnGridDataBoundActive()
    {
        if ( !_initialLoadDoneActive )
        {
            _initialLoadDoneActive = true;
            await UpdateVisibleRowCountActiveAsync();
        }
    }

    protected async Task OnGridDataBoundInactive()
    {
        if ( !_initialLoadDoneInactive )
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

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRefActive!.ExportToExcelAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "Excel" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Taken", "success", _report );
    }

    protected async Task ExportToExcelInactive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameInactive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRefInactive!.ExportToExcelAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "Excel" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Taken", "success", _report );
    }

    protected async Task ExportToCsvActive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameActive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRefActive!.ExportToCsvAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "CSV" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Taken", "success", _report );
    }

    protected async Task ExportToCsvInactive()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileNameInactive}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRefInactive!.ExportToCsvAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "CSV" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Taken", "success", _report );
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
        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRefActive!.ExportToPdfAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "PDF" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Taken", "success", _report );
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
        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRefInactive!.ExportToPdfAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "PDF" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Taken", "success", _report );
    }
}
