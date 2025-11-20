using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;

namespace Amusing.Components.Pages;

public partial class PlanningOverview
{
    [Inject] public PlanningXmlExportService PlanningXmlExportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    private async Task ExportToXmlAsync()
    {
        var bytes = await PlanningXmlExportService.GenerateXmlAsync();

        var base64 = Convert.ToBase64String(bytes);
        await JS.InvokeVoidAsync( "downloadFileFromBase64",
            "planning.xml",
            "text/xml",
            base64 );
    }

    private async Task ExportToExcelAsync()
    {
        // LEGE placeholder – vullen we zodra de XML werkt
        await Task.CompletedTask;
    }
}