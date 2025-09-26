using System.Dynamic;
using System.Text.Json;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.QueryBuilder;
using Syncfusion.Blazor.RichTextEditor;

namespace Amusing.Components.Pages;

public partial class Mailings_Templates
{
    private bool _isLoading = false;
    private bool _disposed = false;
    private bool _showRTE = true;
    private readonly string _pageName = "Mail Templates";
    protected static readonly string[] InFields = { "Festival", "Role", "Volunteer" };

    private List<TemplatesListModel> TemplatesList { get; set; } = [ ];
    private List<RecipientListModel> RecipientsList { get; set; } = [ ];
    private List<string> AvailableFields { get; set; } = [ ];
    private List<SlashMenuItemModel> _slashMenuItems = new();
    private List<SlashMenuItemModel> SlashMenuItems
    {
        get => _slashMenuItems;
        set => _slashMenuItems = value;
    }
    private SfRichTextEditor _rte;
    private SfAutoComplete<string, string> _subjectAuto;
    private int _lastCaretPos = 0;
    private Dictionary<string, object> _subjectHtmlAttr = new() { { "id", "subjectAutoInput" } };

    private TemplatesListModel _selectedTemplatesList;

    private EditContext? _editContext;
    private readonly CancellationTokenSource _cts = new();
    private CancellationTokenSource _loadCts = new();

    private uint? RecipientListId
    {
        get => _selectedTemplatesList?.RecipientListId;
        set
        {
            if ( _selectedTemplatesList != null && _selectedTemplatesList.RecipientListId != value )
            {
                _selectedTemplatesList.RecipientListId = value;
                _ = LoadRecipientDataAsync( value );
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
        _mappingService = new FieldMappingService();

        _isLoading = true;

        TemplatesList = await MailingService.GetMailTemplatesAsync();
        RecipientsList = await MailingService.GetRecipientListsAsync();

        // Add a fictive None row
        TemplatesList.Insert( 0, new TemplatesListModel { TemplateId = 0, TemplateName = "geen", RecipientListId = 0, RecipientListName = "geen" } );
        RecipientsList.Insert( 0, new RecipientListModel { ListId = null, ListName = "geen" } );

        // Default selection
        _selectedTemplatesList = TemplatesList.FirstOrDefault() ?? new TemplatesListModel();

        // Replace old DB tokens with internal template keys + NL labels
        _selectedTemplatesList.TemplateSubject = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateSubject );
        _selectedTemplatesList.TemplateContent = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateContent );

        _editContext = new EditContext( _selectedTemplatesList );
        _isLoading = false;
    }

    protected async Task Save()
    {
        //Cnvert Dutch labels with Englisch field keys.
        _selectedTemplatesList.TemplateSubject = _mappingService.ReplaceLabelsWithKeys( _selectedTemplatesList.TemplateSubject );
        _selectedTemplatesList.TemplateContent = _mappingService.ReplaceLabelsWithKeys( _selectedTemplatesList.TemplateContent );

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
        //_editContext = new EditContext( SelectedRecipientsList );
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

        _selectedTemplatesList = template;

        if ( string.IsNullOrEmpty( template.RecipientListId.ToString() ) )
        {
            _selectedTemplatesList.RecipientListId = 0;
        }

        _selectedTemplatesList.TemplateSubject = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateSubject, true );
        _selectedTemplatesList.TemplateContent = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateContent, true );

        _editContext = new EditContext( _selectedTemplatesList );

        await InvokeAsync( StateHasChanged );
    }

    private uint? SelectedTemplatesListId
    {
        get => _selectedTemplatesList?.TemplateId;
        set
        {
            if ( value.HasValue && TemplatesList != null )
            {
                _selectedTemplatesList = TemplatesList.FirstOrDefault( t => t.TemplateId == value.Value )
                    ?? new TemplatesListModel();

                // Force “geen” as RecipientListId When there is no RecipientList
                if ( !RecipientsList.Any( r => r.ListId == _selectedTemplatesList.RecipientListId ) )
                {
                    _selectedTemplatesList.RecipientListId = null;
                }

                _selectedTemplatesList.TemplateSubject = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateSubject, true );
                _selectedTemplatesList.TemplateContent = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateContent, true );

                _editContext = new EditContext( _selectedTemplatesList );
                InvokeAsync( StateHasChanged );

                _ = LoadRecipientDataAsync( _selectedTemplatesList.RecipientListId );
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

                if ( _selectedTemplatesList?.RecipientListId != null )
                {
                    await LoadRecipientDataAsync( _selectedTemplatesList.RecipientListId );
                }
                _selectedTemplatesList.TemplateSubject = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateSubject, true );
                _selectedTemplatesList.TemplateContent = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateContent, true );
                await JSRuntime.InvokeVoidAsync( "rteHelpers.registerInput", "subjectAutoInput" );
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
        _selectedTemplatesList = TemplatesList.FirstOrDefault() ?? new TemplatesListModel();
        _editContext = new EditContext( _selectedTemplatesList );

        await InvokeAsync( StateHasChanged );
    }

    private async Task LoadRecipientDataAsync( uint? recipientListId )
    {
        // Cancel previous load
        _loadCts.Cancel();
        _loadCts.Dispose();
        _loadCts = new CancellationTokenSource();
        CancellationToken token = _loadCts.Token;

        if ( recipientListId == null )
        {
            AvailableFields.Clear();
            StateHasChanged();
            return;
        }

        RecipientListModel? selectedList = RecipientsList.FirstOrDefault( r => r.ListId == recipientListId.Value );
        if ( selectedList == null )
        {
            AvailableFields.Clear();
            StateHasChanged();
            return;
        }

        // Convert old filter JSON to new QueryBuilder JSON
        if ( string.IsNullOrWhiteSpace( selectedList.ListQuery ) && !string.IsNullOrWhiteSpace( selectedList.ListFilter ) )
        {
            selectedList.ListQuery = QueryBuilderJsonConverter.OldToNew( selectedList.ListFilter );
        }

        if ( string.IsNullOrWhiteSpace( selectedList.ListQuery ) )
        {
            AvailableFields.Clear();
            StateHasChanged();
            return;
        }

        try
        {
            // JSON → RuleModel
            RuleModel? rules = JsonSerializer.Deserialize<RuleModel>( selectedList.ListQuery );
            if ( rules == null )
            {
                AvailableFields.Clear();
                StateHasChanged();
                return;
            }

            NormalizeOperators( rules );
            ForceAndCondition( rules );

            string sourceChecked = selectedList.ListSource switch
            {
                MailingService.RecipientListSource.Persons => "persons",
                MailingService.RecipientListSource.Groups => "groups",
                _ => "persons"
            };

            string fullQuery = QueryBuilderHelper.DetermineQueryFromRules(rules, sourceChecked);

            List<ExpandoObject> dynamicRecipients = await MailingService.GetDynamicRecipientsAsync( fullQuery );

            // Get the available fields for the template, based on the selected RecipientList
            // Banned fields are stripped from the list
            // Fields are Translated for the user
            if ( dynamicRecipients.FirstOrDefault() is IDictionary<string, object> firstRow )
            {
                AvailableFields = [ .. _mappingService.GetAvailableLabels( firstRow.Keys ) ];

                SlashMenuItems = [ .. AvailableFields.Select( f => new SlashMenuItemModel { Text = f, IconCss = "e-icons e-named-set", GroupBy = "Variabelen:" } ) ];

                // To be sure the SlashMenuItems become visible it is necesary
                // to rerender the RTE, so disable it and reaneble it again
                _showRTE = false;
                StateHasChanged();
                _showRTE = true;
                StateHasChanged();
            }
            else
            {
                AvailableFields.Clear();
            }

            StateHasChanged();
        }
        catch ( OperationCanceledException )
        {
            // Load werd gecanceld: doe niets
        }
        catch ( Exception ex )
        {
            Console.Error.WriteLine( $"Error processing recipient list {recipientListId}: {ex.Message}" );
            AvailableFields.Clear();
            StateHasChanged();
        }
    }

    protected void ForceAndCondition( RuleModel? rule )
    {
        if ( rule == null )
        {
            return;
        }

        if ( rule.Rules != null && rule.Rules.Any() )
        {
            rule.Condition = "and";
            foreach ( RuleModel? child in rule.Rules )
            {
                ForceAndCondition( child );
            }
        }
        else
        {
            rule.Condition = null;
        }
    }

    private static void NormalizeOperators( RuleModel rule )
    {
        if ( rule == null )
        {
            return;
        }

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
    }

    private FieldMappingService _mappingService;

    public async Task OnSlashMenuItemSelect( SlashMenuSelectEventArgs args ) => await _rte.ExecuteCommandAsync( CommandName.InsertHTML, args.ItemData.Text );

    private async Task OnFiltering( FilteringEventArgs args )
    {
        // voorkom default filtering
        args.PreventDefaultAction = true;

        // huidige waarde en caret ophalen via JS
        string currentValue = await JSRuntime.InvokeAsync<string>("rteHelpers.getActiveValue");
        int caret = await JSRuntime.InvokeAsync<int>("rteHelpers.getLastCaret");

        // fallback: caret > 0, anders laatste bekende
        if ( caret <= 0 )
        {
            caret = _lastCaretPos;
        }

        // sanity-check boundaries
        caret = Math.Clamp( caret, 0, currentValue?.Length ?? 0 );

        bool trigger = false;

        // als we precies op een '{' staan of args.Text eindigt met '{'
        if ( !string.IsNullOrEmpty( currentValue ) && caret > 0 && currentValue [ caret - 1 ] == '{' )
        {
            trigger = true;
        }
        else if ( !string.IsNullOrEmpty( args.Text ) && args.Text.EndsWith( "{" ) )
        {
            trigger = true;
        }

        if ( trigger )
        {
            // laat AutoComplete zien met volledige lijst
            await _subjectAuto.FilterAsync( AvailableFields );
            await _subjectAuto.ShowPopupAsync();
        }
        else
        {
            // sluit de lijst, geen items tonen
            await _subjectAuto.FilterAsync( new List<string>() );
        }

        // update fallback caret
        _lastCaretPos = caret;
    }

    private async Task OnValueSelect( SelectEventArgs<string> args )
    {
        args.Cancel = true;

        string selected = args.ItemData ?? string.Empty;
        string current = _selectedTemplatesList.TemplateSubject ?? string.Empty;

        // zorg dat de variabele correct tussen {} staat, maar niet dubbel
        if ( !selected.StartsWith( "{" ) && !selected.EndsWith( "}" ) )
        {
            selected = "{" + selected + "}";
        }

        // caret ophalen
        int caret = await JSRuntime.InvokeAsync<int>("rteHelpers.getLastCaret");
        if ( caret <= 0 )
        {
            caret = _lastCaretPos;
        }

        caret = Math.Clamp( caret, 0, current.Length );

        // check voor spaties voor en na
        bool addSpaceBefore = caret > 0 && current[caret - 1] != ' ' && !selected.StartsWith("{");
        bool addSpaceAfter = caret < current.Length && current[caret] != ' ';

        string insertText = (addSpaceBefore ? " " : "") + selected + (addSpaceAfter ? " " : "");

        // voeg in op caret
        current = current.Insert( caret, insertText );

        // update model en caret
        _selectedTemplatesList.TemplateSubject = current;
        _lastCaretPos = caret + insertText.Length;

        StateHasChanged();

        await JSRuntime.InvokeVoidAsync( "rteHelpers.setCaretById", "subjectAutoInput", _lastCaretPos );
    }

}
