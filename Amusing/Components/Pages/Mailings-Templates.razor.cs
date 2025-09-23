using System.Dynamic;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components.Forms;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.QueryBuilder;
using Syncfusion.Blazor.RichTextEditor;

namespace Amusing.Components.Pages;

public partial class Mailings_Templates
{
    private bool _initialLoadDone = false;
    private bool IsLoading = false;
    private bool _disposed = false;
    private int VisibleRowCount = 0;
    private string PageName = "Mail Templates";

    private List<TemplatesListModel> TemplatesList { get; set; } = [];
    private List<RecipientListModel> RecipientsList { get; set; } = [ ];
    private List<string> AvailableFields { get; set; } = new();

    private TemplatesListModel SelectedTemplatesList;
    
    private EditContext? editContext;
    private CancellationTokenSource _cts = new();

    private uint? RecipientListId
    {
        get => SelectedTemplatesList?.RecipientListId;
        set
        {
            if ( SelectedTemplatesList != null && SelectedTemplatesList.RecipientListId != value )
            {
                SelectedTemplatesList.RecipientListId = value;
                _ = OnRecipientListChanged( value );
            }
        }
    }

    private readonly List<ToolbarItemModel> _rteTools =
	[
		new ToolbarItemModel() { Command = ToolbarCommand.Undo },
        new ToolbarItemModel() { Command = ToolbarCommand.Redo },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Bold },
        new ToolbarItemModel() { Command = ToolbarCommand.Italic },
        new ToolbarItemModel() { Command = ToolbarCommand.Underline },
        new ToolbarItemModel() { Command = ToolbarCommand.StrikeThrough },
        new ToolbarItemModel() { Command = ToolbarCommand.SuperScript },
        new ToolbarItemModel() { Command = ToolbarCommand.SubScript },
        new ToolbarItemModel() { Command = ToolbarCommand.Blockquote },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.LowerCase },
        new ToolbarItemModel() { Command = ToolbarCommand.UpperCase },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Formats },
        new ToolbarItemModel() { Command = ToolbarCommand.FontName },
        new ToolbarItemModel() { Command = ToolbarCommand.FontSize },
        new ToolbarItemModel() { Command = ToolbarCommand.FontColor },
        new ToolbarItemModel() { Command = ToolbarCommand.BackgroundColor },
        new ToolbarItemModel() { Command = ToolbarCommand.ClearFormat },
        new ToolbarItemModel() { Command = ToolbarCommand.HorizontalSeparator },
        new ToolbarItemModel() { Command = ToolbarCommand.Alignments },
        new ToolbarItemModel() { Command = ToolbarCommand.NumberFormatList },
        new ToolbarItemModel() { Command = ToolbarCommand.BulletFormatList },
        new ToolbarItemModel() { Command = ToolbarCommand.Indent },
        new ToolbarItemModel() { Command = ToolbarCommand.Outdent },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.CreateTable },
        new ToolbarItemModel() { Command = ToolbarCommand.CreateLink },
        new ToolbarItemModel() { Command = ToolbarCommand.HorizontalLine },
        new ToolbarItemModel() { Command = ToolbarCommand.Image },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.SourceCode },
        new ToolbarItemModel() { Command = ToolbarCommand.CreateTable },
        new ToolbarItemModel() { Command = ToolbarCommand.FullScreen }
    ];

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        TemplatesList = await MailingService.GetMailTemplatesAsync();
        RecipientsList = await MailingService.GetRecipientListsAsync();

        // Add a fictive None row
        TemplatesList.Insert( 0, new TemplatesListModel { TemplateId = 0, TemplateName = "geen", RecipientListId = 0, RecipientListName = "geen" } );
        RecipientsList.Insert( 0, new RecipientListModel { ListId = null, ListName = "geen" } );

        // Default selection
        SelectedTemplatesList = TemplatesList.FirstOrDefault() ?? new TemplatesListModel();

        // Set RecipientListId to null when RecipientListId is 0
        if ( SelectedTemplatesList.RecipientListId == 0 )
            SelectedTemplatesList.RecipientListId = null;

        editContext = new EditContext( SelectedTemplatesList );

        IsLoading = false;
    }

    //protected async Task OnGridDataBound()
    //{
    //    if ( !_initialLoadDone && TemplatesList?.Any() == true )
    //    {
    //        _initialLoadDone = true;
    //        await UpdateVisibleRowCountAsync();

    //        // Select first row in the SfGrid
    //        if ( GridRef != null )
    //        {
    //            await GridRef.SelectRowAsync( 0 );
    //        }

    //        // filll QueryBuilder with query data from selected row
    //        //if ( TemplatesList.Count > 0 )
    //        //{
    //        //    await SelectTemplatesListAsync( TemplatesList [ 0 ] );
    //        //}
    //    }
    //}

    protected async Task OnRowSelected( RowSelectEventArgs<TemplatesListModel> args )
    {
        await SelectTemplateListAsync( args.Data );
    }

    //protected async Task UpdateVisibleRowCountAsync()
    //{
    //    if ( GridRef is not null )
    //    {
    //        List<TemplatesListModel> records = await GridRef.GetCurrentViewRecordsAsync();
    //        await Task.Delay( 150 );
    //        VisibleRowCount = records?.Count ?? 0;
    //        StateHasChanged();
    //    }
    //}

    //public async Task OnInput( InputEventArgs args )
    //{
    //    if ( GridRef != null )
    //    {
    //        await GridRef.SearchAsync( args.Value );
    //        await Task.Delay( 50 );
    //        await UpdateVisibleRowCountAsync();
    //    }
    //}

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

    protected async Task SelectTemplateListAsync( TemplatesListModel template )
    {
        if ( template == null )
        {
            return;
        }

        SelectedTemplatesList = template;
        
        if ( string.IsNullOrEmpty( template.RecipientListId.ToString() ) )
            SelectedTemplatesList.RecipientListId = 0;

        editContext = new EditContext( SelectedTemplatesList );    

        await InvokeAsync( StateHasChanged );
    }

    private uint? SelectedTemplatesListId
    {
        get => SelectedTemplatesList?.TemplateId;
        set
        {
            if ( value.HasValue && TemplatesList != null )
            {
                SelectedTemplatesList = TemplatesList.FirstOrDefault( t => t.TemplateId == value.Value )
                    ?? new TemplatesListModel();

                // Force “geen” as RecipientListId When there is no RecipientList
                if ( !RecipientsList.Any( r => r.ListId == SelectedTemplatesList.RecipientListId ) )
                {
                    SelectedTemplatesList.RecipientListId = null;
                }

                // Re-create the EditContext to bind the new template
                editContext = new EditContext( SelectedTemplatesList );

                // Force UI to refresh
                InvokeAsync( StateHasChanged );
            }
        }
    }

    protected override async Task OnAfterRenderAsync( bool firstRender )
    {
        if ( firstRender && !_disposed )
        {
            try
            {
                await LoadTemplatesAsync( _cts.Token );
            }
            catch ( OperationCanceledException ) { }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }

    private async Task LoadTemplatesAsync( CancellationToken token )
    {
        TemplatesList = await MailingService.GetMailTemplatesAsync();
        RecipientsList = await MailingService.GetRecipientListsAsync();
        token.ThrowIfCancellationRequested();

        // Default selection
        SelectedTemplatesList = TemplatesList.FirstOrDefault() ?? new TemplatesListModel();
        editContext = new EditContext( SelectedTemplatesList );

        await InvokeAsync( StateHasChanged );
    }

    private async Task OnRecipientListChanged( uint? listId )
    {
        if ( listId == null )
            return;

        // Vind de geselecteerde lijst
        var selectedList = RecipientsList.FirstOrDefault(r => r.ListId == listId.Value);
        if ( selectedList == null )
            return;

        // Converteer eventueel oude filter naar nieuwe QueryBuilder JSON
        if ( string.IsNullOrWhiteSpace( selectedList.ListQuery ) && !string.IsNullOrWhiteSpace( selectedList.ListFilter ) )
        {
            selectedList.ListQuery = QueryBuilderJsonConverter.OldToNew( selectedList.ListFilter );
        }

        if ( string.IsNullOrWhiteSpace( selectedList.ListQuery ) )
        {
            AvailableFields.Clear();
            return;
        }

        try
        {
            // JSON → RuleModel
            var rules = JsonSerializer.Deserialize<RuleModel>(selectedList.ListQuery);
            if ( rules == null )
            {
                AvailableFields.Clear();
                return;
            }

            // Force all operators to string
            NormalizeOperators( rules );

            // Force AND voor consistentie
            ForceAndCondition( rules );

            // Afleiden van SourceChecked
            string sourceChecked = selectedList.ListSource switch
            {
                MailingService.RecipientListSource.Persons => "persons",
                MailingService.RecipientListSource.Groups => "groups",
                _ => "persons"
            };

            // Maak de echte SQL-query
            string fullQuery = QueryBuilderHelper.DetermineQueryFromRules(rules, sourceChecked);

            // Ophalen van dynamische resultaten
            var dynamicRecipients = await MailingService.GetDynamicRecipientsAsync(fullQuery) ?? new List<ExpandoObject>();

            // Bepaal de beschikbare kolommen
            AvailableFields = dynamicRecipients.FirstOrDefault() is IDictionary<string, object> firstRow
                ? firstRow.Keys.ToList()
                : new List<string>();
        }
        catch ( Exception ex )
        {
            // Fallback bij parsing/fouten
            Console.Error.WriteLine( $"Error processing recipient list {listId}: {ex.Message}" );
            AvailableFields.Clear();
        }
    }

    protected void ForceAndCondition( RuleModel? rule )
    {
        if ( rule == null )
            return;

        if ( rule.Rules != null && rule.Rules.Any() )
        {
            rule.Condition = "and";
            foreach ( var child in rule.Rules )
            {
                ForceAndCondition( child );
            }
        }
        else
        {
            rule.Condition = null;
        }
    }

    private void NormalizeOperators( RuleModel rule )
    {
        if ( rule == null )
            return;

        if ( rule.Operator is JsonElement je )
        {
            // Convert JsonElement to string safely
            rule.Operator = je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.GetRawText(), // numbers als string
                JsonValueKind.Null => null,
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => je.GetRawText()
            };
        }

        // Recursief voor subrules
        if ( rule.Rules != null )
        {
            foreach ( var subRule in rule.Rules )
            {
                NormalizeOperators( subRule );
            }
        }
    }

}
