using Syncfusion.Blazor.Grids;
namespace Amusing.Components.Pages;
public partial class MaintenanceSubscriptions
{
    [Microsoft.AspNetCore.Components.Inject] protected Amusing.Services.RegistrationPlanningExportService PlanningExport {get;set;}=default!;
    [Microsoft.AspNetCore.Components.Inject] protected Microsoft.JSInterop.IJSRuntime ExportJs {get;set;}=default!;
    [Microsoft.AspNetCore.Components.Inject] protected ILogger<MaintenanceSubscriptions> ExportLogger {get;set;}=default!;
    private bool planningExportBusy;
    protected async Task ExportPlanningAsync()
    {
        if(planningExportBusy || !uint.TryParse(SelectedEditionId,out var festival)) return;
        planningExportBusy=true;
        try
        {
            var data=await PlanningExport.ReadAsync(festival);
            var bytes=Amusing.Services.RegistrationPlanningExport.CreateWorkbook(data);
            var file=$"Planningexport-{data.Year}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
            await Microsoft.JSInterop.JSRuntimeExtensions.InvokeVoidAsync(ExportJs,"downloadFile",file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",Convert.ToBase64String(bytes));
            await ToastService.ShowExportCompletedAsync(file,"Excel");
            await LoggingService.WriteUserActionAsync("Beheer","Inschrijvingen","success",$"<_userName> heeft planningexport voor editie {data.Year} geëxporteerd");
        }
        catch(Exception error)
        {
            ExportLogger.LogError(error,"Planningexport voor festival {FestivalId} mislukt",festival);
            await ToastService.ShowErrorAsync("De planningexport kon niet worden gemaakt. Probeer opnieuw of neem contact op met de beheerder.");
        }
        finally {planningExportBusy=false;}
    }
    private static List<GridColumn> RegistrationExportColumns()=>new()
    {
        new(){Field="Datum",HeaderText="Datum",Format="dd-MM-yyyy",Type=ColumnType.Date},
        new(){Field="Naam",HeaderText="Naam"},
        new(){Field="Stad",HeaderText="Stad"},
        new(){Field="AvailableFrom",HeaderText="Beschikbaar van"},
        new(){Field="AvailableUntil",HeaderText="Beschikbaar tot"},
        new(){Field="Podium",HeaderText="Podium"},
        new(){Field="Zangers",HeaderText="Zangers"},
        new(){Field="Genre",HeaderText="Genre"},
        new(){Field="TeBetalen",HeaderText="Te betalen (€)",Format="N2",TextAlign=TextAlign.Right},
        new(){Field="Factuurdatum",HeaderText="Factuurdatum",Format="dd-MM-yyyy",Type=ColumnType.Date},
        new(){Field="BetaaldBedrag",HeaderText="Betaald",Format="N2",TextAlign=TextAlign.Right},
        new(){Field="Betaaldatum",HeaderText="Betaaldatum",Format="dd-MM-yyyy",Type=ColumnType.Date},
        new(){Field="Bevestigingsdatum",HeaderText="Bevestigd",Format="dd-MM-yyyy",Type=ColumnType.Date},
        new(){Field="AcapellaBattle",HeaderText="AcapellaBattle"},
        new(){Field="Binnen",HeaderText="Binnen"},
        new(){Field="Buiten",HeaderText="Buiten"},
        new(){Field="Afhaakdatum",HeaderText="Afgehaakt",Format="dd-MM-yyyy",Type=ColumnType.Date},
    };
    // Export functions
    protected async Task ExportToExcel()
    {
        var exportProps = new ExcelExportProperties
        {
            Columns = RegistrationExportColumns(),
            FileName = $"Inschrijvingen {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToExcelAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "Excel" );
        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Beheer", "Inschrijvingen", "success", _report );
    }

    protected async Task ExportToCsv()
    {
        var exportProps = new ExcelExportProperties
        {
            Columns = RegistrationExportColumns(),
            FileName = $"Inschrijvingen {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToCsvAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "CSV" );
        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Beheer", "Inschrijvingen", "success", _report );
    }

    protected async Task ExportToPdf()
    {
        var exportProps = new PdfExportProperties
        {
            Columns = RegistrationExportColumns(),
            FileName = $"Inschrijvingen {SelectedEditionText}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize=PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await ToastService.ShowExportStartedAsync( exportProps.FileName );
        await GridRef!.ExportToPdfAsync( exportProps );
        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "PDF" );
        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Beheer", "Inschrijvingen", "success", _report );
    }

}
