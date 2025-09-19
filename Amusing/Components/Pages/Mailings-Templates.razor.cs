using System.Text.Json;
using System.Text.Json.Serialization;

using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components.Forms;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.QueryBuilder;

namespace Amusing.Components.Pages;

public partial class Mailings_Templates
{
    private bool _initialLoadDone = false;
    private bool IsLoading = false;
    private EditContext? editContext;
    private int VisibleRowCount = 0;   
    private List<TemplatesListModel> TemplatesList { get; set; } = [];
    private SfGrid<TemplatesListModel>? GridRef;
    private TemplatesListModel? SelectedTemplatesList;
    private string PageName = "Mail Templates";

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        //Get all Templateslists
        TemplatesList = await MailingService.GetMailTemplatesAsync();
        SelectedTemplatesList = TemplatesList.FirstOrDefault();
        if ( SelectedTemplatesList != null )
        {
            editContext = new EditContext( SelectedTemplatesList );
        }

        //if ( SelectedTemplatesList != null )
        //{
        //    if ( string.IsNullOrWhiteSpace( SelectedTemplatesList.ListQuery ) && !string.IsNullOrWhiteSpace( SelectedTemplatesList.ListFilter ) )
        //    {
        //        SelectedTemplatesList.ListQuery = QueryBuilderJsonConverter.OldToNew( SelectedTemplatesList.ListFilter );
        //    }

        //    LoadRulesIntoQueryBuilder( SelectedTemplatesList.ListQuery );
        //}

        IsLoading = false;
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone && TemplatesList?.Any() == true )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();

            // Select first row in the SfGrid
            if ( GridRef != null )
            {
                await GridRef.SelectRowAsync( 0 );
            }

            // filll QueryBuilder with query data from selected row
            //if ( TemplatesList.Count > 0 )
            //{
            //    await SelectTemplatesListAsync( TemplatesList [ 0 ] );
            //}
        }
    }

    protected async Task OnRowSelected( RowSelectEventArgs<TemplatesListModel> args )
    {
        //await SelectTemplatesListAsync( args.Data );
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if ( GridRef is not null )
        {
            List<TemplatesListModel> records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    public async Task OnInput( InputEventArgs args )
    {
        if ( GridRef != null )
        {
            await GridRef.SearchAsync( args.Value );
            await Task.Delay( 50 );
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task Save()
    {
        //if ( SelectedRecipientsList is null )
        //{
        //    return;
        //}

        //RuleModel? rules = personsQueryBuilder?.GetRules();

        //// Force AND
        //ForceAndCondition( rules );

        //SelectedRecipientsList.ListQuery = JsonSerializer.Serialize( rules, new JsonSerializerOptions
        //{
        //    WriteIndented = true,
        //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        //} );

        //if ( SelectedRecipientsList.ListId != 0 )
        //{
        //    await MailingService.UpdateRecipientQueryAsync( SelectedRecipientsList );
        //}
        //else
        //{
        //    // Save the new group and get the new group Id
        //    var savedId = await MailingService.AddRecipientQueryAsync(SelectedRecipientsList);

        //    // Refresh the list
        //    RecipientsList = await MailingService.GetRecipientListsAsync();
        //    await Task.Delay( 50 );
        //    if ( GridRef != null )
        //    {
        //        await GridRef.Refresh();
        //    }

        //    // Search the modified record
        //    var index = RecipientsList.FindIndex(s => s.ListId == savedId);
        //    if ( index >= 0 )
        //    {
        //        SelectedRecipientsList = RecipientsList [ index ];
        //        await GridRef.SelectRowAsync( index );
        //    }
        //}
    }

    protected async Task AddNew()
    {
        //RecipientListModel newRecipientsList = new()
        //{
        //    ListId = 0,
        //    ListName = ""
        //};

        //if ( GridRef != null )
        //{
        //    await GridRef.AddRecordAsync( newRecipientsList, 0 );
        //    await GridRef.SelectRowAsync( 0 );
        //}

        //SelectedRecipientsList = newRecipientsList;
        //editContext = new EditContext( SelectedRecipientsList );
        //StateHasChanged();
    }

    protected async Task Delete()
    {
    //    if ( SelectedRecipientsList == null || SelectedRecipientsList.ListId == 0 )
    //    {
    //        return;
    //    }

    //    await MailingService.DeleteRecipientQueryAsync( SelectedRecipientsList.ListId );

    //    // Refresh the list
    //    RecipientsList = await MailingService.GetRecipientListsAsync();
    //    await Task.Delay( 50 );
    //    if ( GridRef != null )
    //    {
    //        await GridRef.Refresh();
    //    }
    }
}
