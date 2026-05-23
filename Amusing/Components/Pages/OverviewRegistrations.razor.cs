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

public partial class OverviewRegistrations : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] protected RegistrationService RegistrationService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;

    protected SfGrid<RegistrationModel> GridRef;
    protected List<Edition> Editions = [];
    protected List<RegistrationModel> RegistrationList = [];
    protected string? SelectedEditionId;
    protected int VisibleRowCount = 0;

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
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (RegistrationList?.Count > 0)
            {
                await UpdateVisibleRowCount();
            }
        }
    }

    // Whenever the selected edition changes the datagrid has to be updated
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
    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        // Count the Number of visible rows
        await Task.Delay( 200 ); // Short delay to handle fast typers
        await UpdateVisibleRowCount();
    }

    // Export functions
    protected async Task ExportToExcel()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"Inschrijvingen {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToExcelAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "Excel" );
        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Overzichten", "Inschrijvingen", "success", _report );
    }

    protected async Task ExportToCsv()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"Inschrijvingen {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToCsvAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "CSV" );
        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Overzichten", "Inschrijvingen", "success", _report );
    }

    protected async Task ExportToPdf()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"Inschrijvingen {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize=PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToPdfAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "PDF" );
        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Overzichten", "Inschrijvingen", "success", _report );
    }

    // Visible row count (Number of records on screen)
    protected async Task UpdateVisibleRowCount()
    {
        var data = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = data?.Count ?? 0;
    }
}
