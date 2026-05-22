using System.Collections.Generic;
using System.Threading.Tasks;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;

namespace Amusing.Components.Pages;

public partial class OverviewEmailAddresses : ComponentBase
{
    [Inject] private EmailAddressesService EmailAddressesService { get; set; } = default!;
    [Inject] private LoggingService LoggingService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    protected string? SelectedCountry;
    protected bool IsInitialized;

    protected SfGrid<EmailAddressesModel>? NewsletterEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? AllKnownEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? NewlyAddedEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? OldEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? PreviousEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? UpcommingEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? QueueUpcommingEmailAddressesGridRef;
    protected SfGrid<EmailAddressesModel>? IncompleteEmailAddressesGridRef;

    protected List<EmailAddressesModel> AllNewsletterEmailAddressesList = new();
    protected List<EmailAddressesModel> AllAllKnownEmailAddressesList = new();
    protected List<EmailAddressesModel> AllNewlyAddedEmailAddressesList = new();
    protected List<EmailAddressesModel> AllOldEmailAddressesList = new();
    protected List<EmailAddressesModel> AllPreviousEmailAddressesList = new();
    protected List<EmailAddressesModel> AllUpcommingEmailAddressesList = new();
    protected List<EmailAddressesModel> AllQueueUpcommingEmailAddressesList = new();
    protected List<EmailAddressesModel> AllIncompleteEmailAddressesList = new();

    protected List<EmailAddressesModel> FilteredNewsletterEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredAllKnownEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredNewlyAddedEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredOldEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredPreviousEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredUpcommingEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredQueueUpcommingEmailAddressesList = new();
    protected List<EmailAddressesModel> FilteredIncompleteEmailAddressesList = new();

    protected int NewsletterEmailAddressesListVisibleRowCount;
    protected int AllKnownEmailAddressesListVisibleRowCount;
    protected int NewlyAddedEmailAddressesListVisibleRowCount;
    protected int OldEmailAddressesListVisibleRowCount;
    protected int PreviousEmailAddressesListVisibleRowCount;
    protected int UpcommingEmailAddressesListVisibleRowCount;
    protected int QueueUpcommingEmailAddressesListVisibleRowCount;
    protected int IncompleteEmailAddressesListVisibleRowCount;


    public class Countries
    {
        public string Code { get; set; } = "nl";
        public string Name { get; set; } = "Nederland";
    }

    protected List<Countries> CountryList =
	[
		new() { Code= "nl", Name= "Nederland" },
        new Countries() { Code= "de", Name= "Duitsland" },
        new Countries() { Code= "uk", Name= "Overige" },
    ];

    protected async Task OnCountryChanged( string selectedCountry )
    {
        if ( string.IsNullOrWhiteSpace( selectedCountry ) )
            return;

        SelectedCountry = selectedCountry;

        await LoadNewsletterEmailAddressesAsync();
        ApplyCountryFilters();
        NewsletterEmailAddressesListVisibleRowCount = FilteredNewsletterEmailAddressesList.Count;
        AllKnownEmailAddressesListVisibleRowCount = FilteredAllKnownEmailAddressesList.Count;
        NewlyAddedEmailAddressesListVisibleRowCount = FilteredNewlyAddedEmailAddressesList.Count;
        OldEmailAddressesListVisibleRowCount = FilteredOldEmailAddressesList.Count;
        StateHasChanged(); // Force UI rerender to initializeall Refs

        if ( IsInitialized )
        {
            await NewsletterEmailAddressesGridRef.Refresh();
            await AllKnownEmailAddressesGridRef.Refresh();
            await NewlyAddedEmailAddressesGridRef.Refresh();
            await OldEmailAddressesGridRef.Refresh();
        }
    }

    protected async Task LoadNewsletterEmailAddressesAsync()
    {
        if ( SelectedCountry != "" )
        {
            AllNewsletterEmailAddressesList = await EmailAddressesService.GetNewsletterEmailAddressesAsync();
            AllAllKnownEmailAddressesList = await EmailAddressesService.GetAllKnownEmailAddressesAsync();
            AllNewlyAddedEmailAddressesList = await EmailAddressesService.GetNewlyAddedEmailAddressesAsync();
            AllOldEmailAddressesList = await EmailAddressesService.GetOldEmailAddressesAsync();
            AllPreviousEmailAddressesList = await EmailAddressesService.GetPreviousEmailAddressesAsync();
            AllUpcommingEmailAddressesList = await EmailAddressesService.GetUpcommingEmailAddressesAsync();
            AllQueueUpcommingEmailAddressesList = await EmailAddressesService.GetQueueUpcommingEmailAddressesAsync();
            AllIncompleteEmailAddressesList = await EmailAddressesService.GetIncompleteEmailAddressesAsync();

            NewsletterEmailAddressesListVisibleRowCount = AllNewsletterEmailAddressesList.Count;
            AllKnownEmailAddressesListVisibleRowCount = AllAllKnownEmailAddressesList.Count;
            NewlyAddedEmailAddressesListVisibleRowCount = AllNewlyAddedEmailAddressesList.Count;
            OldEmailAddressesListVisibleRowCount = AllOldEmailAddressesList.Count;
            PreviousEmailAddressesListVisibleRowCount = AllPreviousEmailAddressesList.Count;
            UpcommingEmailAddressesListVisibleRowCount = AllUpcommingEmailAddressesList.Count;
            QueueUpcommingEmailAddressesListVisibleRowCount = AllQueueUpcommingEmailAddressesList.Count;
            IncompleteEmailAddressesListVisibleRowCount = AllIncompleteEmailAddressesList.Count;
        }
    }

    // Apply filtering for the Datasurces of the DataGrids
    protected void ApplyCountryFilters()
    {
        if ( SelectedCountry == "nl" || SelectedCountry == "de" )
        {
            FilteredNewsletterEmailAddressesList = AllNewsletterEmailAddressesList.Where( v => v.Land.ToLower() == SelectedCountry ).ToList();
            FilteredAllKnownEmailAddressesList = AllAllKnownEmailAddressesList.Where( v => v.Land.ToLower() == SelectedCountry ).ToList();
            FilteredNewlyAddedEmailAddressesList = AllNewlyAddedEmailAddressesList.Where( v => v.Land.ToLower() == SelectedCountry ).ToList();
            FilteredOldEmailAddressesList = AllOldEmailAddressesList.Where( v => v.Land.ToLower() == SelectedCountry ).ToList();
        }
        else
        {
            FilteredNewsletterEmailAddressesList = AllNewsletterEmailAddressesList.Where( v => v.Land.ToLower() != "nl" && v.Land.ToLower() != "de" ).ToList();
            FilteredAllKnownEmailAddressesList = AllAllKnownEmailAddressesList.Where( v => v.Land.ToLower() != "nl" && v.Land.ToLower() != "de" ).ToList();
            FilteredNewlyAddedEmailAddressesList = AllNewlyAddedEmailAddressesList.Where( v => v.Land.ToLower() != "nl" && v.Land.ToLower() != "de" ).ToList();
            FilteredOldEmailAddressesList = AllOldEmailAddressesList.Where( v => v.Land.ToLower() != "nl" && v.Land.ToLower() != "de" ).ToList();

        }

        NewsletterEmailAddressesListVisibleRowCount = FilteredNewsletterEmailAddressesList.Count;
        AllKnownEmailAddressesListVisibleRowCount = FilteredAllKnownEmailAddressesList.Count;
        NewlyAddedEmailAddressesListVisibleRowCount = FilteredNewlyAddedEmailAddressesList.Count;
        OldEmailAddressesListVisibleRowCount = FilteredOldEmailAddressesList.Count;
    }

    // Export functions
    protected async Task ExportToExcel( string fileType )
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{GetFileName(fileType)} {(SelectedCountry == "uk" ? "Overige" : SelectedCountry)}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx",
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );

        switch ( fileType )
        {
            case "newsletter":
                await NewsletterEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "allknown":
                await AllKnownEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "newlyadded":
                await NewlyAddedEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "knownnotregistered":
                await OldEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "registeredprevious":
                await PreviousEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "registeredupcomming":
                await UpcommingEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "queueupcomming":
                await QueueUpcommingEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
            case "incomplete":
                await IncompleteEmailAddressesGridRef!.ExportToExcelAsync( exportProps );
                break;
        }


        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "Excel" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Overzichten", "E-Mailadressen", "success", _report );



    }

    protected async Task ExportToCsv( string fileType = "Export" )
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{GetFileName(fileType)} {(SelectedCountry == "uk" ? "Overige" : SelectedCountry)}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv",
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );

        switch ( fileType )
        {
            case "newsletter":
                await NewsletterEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "allknown":
                await AllKnownEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "newlyadded":
                await NewlyAddedEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "knownnotregistered":
                await OldEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "registeredprevious":
                await PreviousEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "registeredupcomming":
                await UpcommingEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "queueupcomming":
                await QueueUpcommingEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
            case "incomplete":
                await IncompleteEmailAddressesGridRef!.ExportToCsvAsync( exportProps );
                break;
        }

        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "CSV" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Overzichten", "E-Mailadressen", "success", _report );

    }

    protected async Task ExportToPdf( string fileType = "Export" )
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"{GetFileName(fileType)} {(SelectedCountry == "uk" ? "Overige" : SelectedCountry)}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize=PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };

        await ToastService.ShowExportStartedAsync( exportProps.FileName );

        switch ( fileType )
        {
            case "newsletter":
                await NewsletterEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "allknown":
                await AllKnownEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "newlyadded":
                await NewlyAddedEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "knownnotregistered":
                await OldEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "registeredprevious":
                await PreviousEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "registeredupcomming":
                await UpcommingEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "queueupcomming":
                await UpcommingEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
            case "incomplete":
                await IncompleteEmailAddressesGridRef!.ExportToPdfAsync( exportProps );
                break;
        }

        await ToastService.ShowExportCompletedAsync( exportProps.FileName, "PDF" );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Overzichten", "E-Mailadressen", "success", _report );

    }

    protected string GetFileName( string fileType )
    {
        string filename = "unknown";

		filename = fileType switch
		{
			"newsletter" => "Nieuwsbrief mail adressen",
			"allknown" => "Bekende groepen",
			"newlyadded" => "Toegevoegd niet geregistreerd",
			"knownnotregistered" => "Bestaand niet aangemeld",
			"registeredprevious" => "Groepen vorige festival",
			"registeredupcomming" => "Groepen komende festival",
			"queueupcomming" => "Wachtlijst huidige festival",
			"incomplete" => "Incomplete profielen",
			_ => "Onbekend",
		};
		return filename;
    }
}
