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

public partial class ListPersons : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected PersonService PersonService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDone = false;
    protected SfGrid<PersonOverviewModel> GridRef;
    protected int VisibleRowCount = 0;
    protected List<PersonOverviewModel> Personen = [];
    protected string FileName = "Personen";

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        Personen = await PersonService.GetPersonOverviewAsync();
        IsLoading = false;
    }

    // Manage direct search functionality
    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if ( GridRef is not null )
        {
            await GridRef.Refresh();
            await Task.Delay( 50 );
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task OnGridActionComplete( ActionEventArgs<PersonOverviewModel> args )
    {
        if ( args.RequestType == Syncfusion.Blazor.Grids.Action.Refresh ||
        args.RequestType == Syncfusion.Blazor.Grids.Action.Filtering ||
        args.RequestType == Syncfusion.Blazor.Grids.Action.Paging ||
        args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting )
        {
            await UpdateVisibleRowCountAsync();
        }
    }

    // Export functions
    protected async Task ExportToExcel()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToExcelAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "Excel" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Personen", "success", _report );
    }

    protected async Task ExportToCsv()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToCsvAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "CSV" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Personen", "success", _report );
    }

    protected async Task ExportToPdf()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize=PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToPdfAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "PDF" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Personen", "success", _report );
    }
} 
