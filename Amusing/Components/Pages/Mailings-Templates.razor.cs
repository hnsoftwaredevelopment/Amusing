using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using System.Text.RegularExpressions;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using FluentValidation.Internal;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.QueryBuilder;
using Syncfusion.Blazor.RichTextEditor;
using Syncfusion.XlsIO.Parser.Biff_Records.ObjRecords;

using ChangeEventArgs = Syncfusion.Blazor.Navigations.ChangeEventArgs;

namespace Amusing.Components.Pages;

public partial class Mailings_Templates : ComponentBase, IDisposable
{
	private bool _isLoading = false;
	private bool _disposed = false;
	private SfTextBox _emailTextBox;
	private SfComboBox<int, int> _countComboBox;
	private string _testEmailAddress = "";
	private int _testRecipientCount = 15;
	private bool _showRTE = true;
	private readonly string _pageName = "Mail Templates";
	protected static readonly string[] InFields = { "Festival", "Role", "Volunteer" };

	private List<TemplatesListModel> TemplatesList { get; set; } = [];
	private List<RecipientListModel> RecipientsList { get; set; } = [];
	private List<string> AvailableFields { get; set; } = [ ];

	private List<SlashMenuItemModel> _slashMenuItems = [];
	private List<SlashMenuItemModel> SlashMenuItems
	{
		get => _slashMenuItems;
		set => _slashMenuItems = value;
	}

	private SfRichTextEditor? _rte;
	private SfAutoComplete<string, string>? _subjectAuto;
	private int _lastCaretPos = 0;
	private Dictionary<string, object> _subjectHtmlAttr = new() { { "id", "subjectAutoInput" } };
	private TemplatesListModel _selectedTemplatesList = new();
	private EditContext? _editContext;
	private CancellationTokenSource _cts = new();
	private CancellationTokenSource _loadCts = new();
	private bool _showPreviewDialog = false;
	private bool _showTestDialog = false;
	private List<string> _previewRecipients = [];
	private int _currentPreviewIndex = 0;
	private int _currentRecipientIndex = 0;
	private string _selectedPreviewRecipient = string.Empty;
	private string _previewSubject = string.Empty;
	private string _previewBody = string.Empty;
	private string? _currentRecipientQuery;
	private List<ExpandoObject> _recipientData = [];
	private ExpandoObject? _selectedRecipient;
	private ExpandoObject? SelectedRecipient
	{
		get => _selectedRecipient;
		set
		{
			if ( _selectedRecipient != value )
			{
				_selectedRecipient = value;
				if ( _selectedRecipient != null )
					UpdatePreview( _selectedRecipient );
			}
		}
	}

	private string _rawSubjectTemplate = "";
	private string _rawBodyTemplate = "";
    private readonly ILogger<TransipMailingService> _logger;

    private bool IsValidEmail( string email )
    {
        if ( string.IsNullOrWhiteSpace( email ) )
            return false;

        // At least one @, a dot, and 2-3 letters after the last dot
        var pattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch( email, pattern, RegexOptions.IgnoreCase );
    }

    private bool IsTestMailValid => !string.IsNullOrWhiteSpace( _testEmailAddress ) && IsValidEmail( _testEmailAddress );

	private void OnEmailInput( InputEventArgs args )
	{
        _testEmailAddress = args.Value?.ToString();
        StateHasChanged();
	}

	//private RecipientListFilterModel? _selectedRecipient;

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

	private readonly List<ToolbarItemModel> _rteToolbarItems =
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

	[Inject] public MailingService MailingService { get; set; } = default!;
	[Inject] public IJSRuntime JSRuntime { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		_mappingService = new FieldMappingService();

		_isLoading = true;

		TemplatesList = await MailingService.GetMailTemplatesAsync();
		RecipientsList = await MailingService.GetRecipientListsAsync();

		// Add a fictive None row
		TemplatesList.Insert( 0, new TemplatesListModel { TemplateId = 0, TemplateName = "geen", RecipientListId = 0, RecipientListName = "geen" } );
		RecipientsList.Insert( 0, new RecipientListModel { ListId = 0, ListName = "geen" } );

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
		if ( _selectedTemplatesList is null )
		{
			return;
		}

		// Ensure the RTE has committed the latest changes
		if ( _rte is not null )
		{
			_selectedTemplatesList.TemplateContent = await _rte.GetXhtmlAsync();
		}

		//Convert Dutch labels to Englisch field keys.
		string? _tempSubject = _selectedTemplatesList.TemplateSubject;
		string _tempContent = _selectedTemplatesList.TemplateContent ?? string.Empty;
		_selectedTemplatesList.TemplateSubject = _mappingService.ReplaceLabelsWithKeys( _selectedTemplatesList.TemplateSubject );
		_selectedTemplatesList.TemplateContent = _mappingService.ReplaceLabelsWithKeys( _selectedTemplatesList.TemplateContent );

		if ( _selectedTemplatesList.TemplateId != 0 )
		{
			await MailingService.UpdateTemplateQueryAsync( _selectedTemplatesList );
		}
		else
		{
			// Save the new group and get the new group Id
			uint savedId = await MailingService.AddTemplateQueryAsync(_selectedTemplatesList);

			// Refresh the list
			TemplatesList = await MailingService.GetMailTemplatesAsync();

			// Search the modified record
			int index = TemplatesList.FindIndex(s => s.TemplateId == savedId);
			if ( index >= 0 )
			{
				_selectedTemplatesList = TemplatesList [ index ];
			}
		}

		// Restore the dutch fieldnames in the UI
		_selectedTemplatesList.TemplateSubject = _tempSubject;
		_selectedTemplatesList.TemplateContent = _tempContent;
	}

	protected void AddNew()
	{
		uint tempId = (uint)(TemplatesList.Count(t => t.TemplateId == 0) + 1);

		TemplatesListModel newTemplate = new()
		{
			TemplateId = 0,
			TemplateName = "",
			TemplateSubject = "",
			TemplateContent = "",
			RecipientListId = RecipientsList.FirstOrDefault()?.ListId
		};

		TemplatesList.Add( newTemplate );

		SelectedTemplatesListId = newTemplate.TemplateId;
		_selectedTemplatesList = newTemplate;

		RecipientListId = RecipientsList.FirstOrDefault()?.ListId;

		_editContext = new EditContext( _selectedTemplatesList );

		_selectedTemplatesList.TemplateSubject = "";
		_selectedTemplatesList.TemplateContent = "";
		AvailableFields.Clear();
		SlashMenuItems.Clear();
		_showRTE = false; // force rerender van RTE
		StateHasChanged();
		_showRTE = true;
		StateHasChanged();
	}

	protected async Task Delete()
	{
		if ( _selectedTemplatesList == null || _selectedTemplatesList.TemplateId == 0 )
		{
			return;
		}

		await MailingService.DeleteTemplateQueryAsync( _selectedTemplatesList.TemplateId );

		// Refresh the list
		TemplatesList = await MailingService.GetMailTemplatesAsync();
		_selectedTemplatesList = TemplatesList.FirstOrDefault() ?? new TemplatesListModel();

		// Replace old DB tokens with internal template keys + NL labels
		_selectedTemplatesList.TemplateSubject = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateSubject );
		_selectedTemplatesList.TemplateContent = _mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateContent );

		_editContext = new EditContext( _selectedTemplatesList );
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
		if ( !firstRender || _disposed )
			return;

		if ( firstRender )
		{
			await Task.Delay( 200 ); // tiny grace delay to let Syncfusion finish its init
			await JSRuntime.InvokeVoidAsync( "rteHelpers.updateContextMenu", "rteContent", SlashMenuItems );

			// Register caret tracking for the AutoComplete input
			await JSRuntime.InvokeVoidAsync( "rteHelpers.registerInput", "subjectAutoInput" );

			// Initialize the right-click context menu for the subject input
			//await JSRuntime.InvokeVoidAsync( "rteHelpers.updateContextMenu", "subjectAutoInput", AvailableFields.Select( x => new { text = x } ).ToList() );
			await JSRuntime.InvokeVoidAsync( "rteHelpers.updateContextMenu", "subjectAutoInput", SlashMenuItems );
		}

		try
		{
			var ready = await WaitForRteReadyAsync();
			if ( ready )
				await UpdateRteSlashMenuAsync();
			else
				Debug.WriteLine( "RTE never ready, skipping slash menu." );

			// Load the available templates
			await LoadTemplatesAsync( _cts.Token );

			// Select the first template if none is selected
			if ( TemplatesList?.Any() == true && SelectedTemplatesListId == 0 )
			{
				SelectedTemplatesListId = TemplatesList [ 0 ].TemplateId;
				_selectedTemplatesList = TemplatesList [ 0 ];
			}

			// Load recipient data if the template has a recipient list
			if ( _selectedTemplatesList?.RecipientListId != null )
			{
				await LoadRecipientDataAsync( _selectedTemplatesList.RecipientListId );
			}

			// Replace placeholders in subject and content
			_selectedTemplatesList.TemplateSubject =
				_mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateSubject, true );

			_selectedTemplatesList.TemplateContent =
				_mappingService.ReplaceKeysWithLabels( _selectedTemplatesList.TemplateContent, true );

			// Wait for the RTE to be ready before applying changes
			var rteReady = await WaitForRteReadyAsync(maxAttempts: 15, delayMs: 100);
			if ( rteReady )
			{
				// Update slash menu and re-render the editor if necessary
				await UpdateRteSlashMenuAsync();

				// Register autocomplete or slash functionality for subject input
				if ( JSRuntime is not null )
				{
					await JSRuntime.InvokeVoidAsync( "rteHelpers.registerInput", "subjectAutoInput" );
				}

				// Initialize right-click context menu for subject (optional enhancement)
				if ( JSRuntime is not null )
				{
					await JSRuntime.InvokeVoidAsync( "rteHelpers.updateContextMenu", "rteContent", SlashMenuItems.Select( x => new { text = x.Text } ) );
					await JSRuntime.InvokeVoidAsync( "rteHelpers.updateContextMenu", "subjectAutoInput", SlashMenuItems.Select( x => new { text = x.Text } ) );
				}
			}
			else
			{
				Debug.WriteLine( "RTE not ready after waiting, skipping slash menu initialization." );
			}
		}
		catch ( OperationCanceledException )
		{
			// Ignore cancellation since it may occur during reload
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"OnAfterRenderAsync unexpected: {ex.Message}" );
		}
	}

	public void Dispose()
	{
		_disposed = true;
		_cts.Cancel();
		_cts.Dispose();

		// Also cancel any outstanding recipient-load operation to avoid continuations after disposal.
		try
		{
			_loadCts?.Cancel();
			_loadCts?.Dispose();
		}
		catch { }
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
			_currentRecipientQuery = null;
			await InvokeAsync( StateHasChanged );
			return;
		}

		RecipientListModel? selectedList = RecipientsList.FirstOrDefault(r => r.ListId == recipientListId.Value);
		if ( selectedList == null )
		{
			AvailableFields.Clear();
			_currentRecipientQuery = null;
			await InvokeAsync( StateHasChanged );
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
			_currentRecipientQuery = null;
			await InvokeAsync( StateHasChanged );
			return;
		}

		try
		{
			// JSON → RuleModel
			RuleModel? rules = JsonSerializer.Deserialize<RuleModel>(selectedList.ListQuery);
			if ( rules == null )
			{
				AvailableFields.Clear();
				_currentRecipientQuery = null;
				await InvokeAsync( StateHasChanged );
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

			_currentRecipientQuery = QueryBuilderHelper.DetermineQueryFromRules( rules, sourceChecked );

			List<ExpandoObject> dynamicRecipients = await MailingService.GetDynamicRecipientsAsync(_currentRecipientQuery);

			// Get the available fields for the template, based on the selected RecipientList
			// Banned fields are stripped from the list
			// Fields are Translated for the user
			if ( dynamicRecipients.FirstOrDefault() is IDictionary<string, object> firstRow )
			{
				AvailableFields = _mappingService.GetAvailableLabels( firstRow.Keys );
				SlashMenuItems = AvailableFields.Select( f => new SlashMenuItemModel { Text = f, IconCss = "e-icons e-named-set", GroupBy = "Variabelen:" } ).ToList();

				// update the client-side RTE slash menu items without re-creating the RTE
				await UpdateRteSlashMenuAsync();

				await InvokeAsync( StateHasChanged );
			}
			else
			{
				AvailableFields.Clear();
				_currentRecipientQuery = null;
			}

			await InvokeAsync( StateHasChanged );
		}
		catch ( OperationCanceledException )
		{
			// Load werd gecanceld: doe niets
		}
		catch ( Exception ex )
		{
			Console.Error.WriteLine( $"Error processing recipient list {recipientListId}: {ex.Message}" );
			AvailableFields.Clear();
			_currentRecipientQuery = null;
			await InvokeAsync( StateHasChanged );
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

	private static void NormalizeOperators( RuleModel? rule )
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

	private FieldMappingService _mappingService = new();

	public async Task OnSlashMenuItemSelect( SlashMenuSelectEventArgs args ) => await ( _rte?.ExecuteCommandAsync( CommandName.InsertHTML, args.ItemData.Text ) ?? Task.CompletedTask );

	private async Task OnFiltering( FilteringEventArgs args )
	{
		args.PreventDefaultAction = true;

		string currentValue = await JSRuntime.InvokeAsync<string>("rteHelpers.getActiveValue");
		int caret = await JSRuntime.InvokeAsync<int>("rteHelpers.getLastCaret", "subjectAutoInput");

		if ( caret <= 0 )
		{
			caret = _lastCaretPos;
		}

		caret = Math.Clamp( caret, 0, currentValue?.Length ?? 0 );

		//bool trigger = false;
		bool trigger = !string.IsNullOrEmpty(currentValue)
					   && caret > 0
					   && (currentValue[caret - 1] == '/' || currentValue[caret - 1] == '{');

		if ( trigger )
		{
			if ( _subjectAuto is not null )
			{
				await _subjectAuto.FilterAsync( AvailableFields );
				await _subjectAuto.ShowPopupAsync();
			}
		}
		else
		{
			if ( _subjectAuto is not null )
				await _subjectAuto.FilterAsync( new List<string>() );
		}

		_lastCaretPos = caret;
	}

	private async Task OnValueSelect( SelectEventArgs<string> args )
	{
		args.Cancel = true;

		string selected = args.ItemData ?? string.Empty;
		string current = await JSRuntime.InvokeAsync<string>("rteHelpers.getActiveValue");
		int caret = await JSRuntime.InvokeAsync<int>("rteHelpers.getLastCaret", "subjectAutoInput");

		// Normalize variable format
		selected = selected.Trim( '{', '}' );
		selected = "{" + selected + "}";

		if ( caret <= 0 )
		{
			caret = _lastCaretPos;
		}

		caret = Math.Clamp( caret, 0, current.Length );

		// check voor spaties voor en na
		bool addSpaceBefore = caret > 0 && current[caret - 1] != ' ' && !selected.StartsWith("{");
		bool addSpaceAfter = caret < current.Length && current[caret] != ' ';

		string insertText = (addSpaceBefore ? " " : "") + selected + (addSpaceAfter ? " " : "");

		// When char before caret is a { then remove it
		if ( caret > 0 && ( current [ caret - 1 ] == '{' || current [ caret - 1 ] == '/' ) )
		{
			current = current.Remove( caret - 1, 1 );
			caret--; // possition carret 1 step back
		}

		// voeg in op caret
		string newValue = current.Insert(caret, insertText);
		_selectedTemplatesList.TemplateSubject = newValue;
		_lastCaretPos = caret + insertText.Length;

		await JSRuntime.InvokeVoidAsync( "rteHelpers.setCaretById", "subjectAutoInput", _lastCaretPos );
	}

	protected async Task MailPreview()
	{
		if ( _selectedTemplatesList?.RecipientListId == null )
			return;

		//Convert Dutch labels to Englisch field keys.
		string? _tempSubject = _selectedTemplatesList.TemplateSubject;
		string _tempContent = _selectedTemplatesList.TemplateContent ?? string.Empty;
		//_selectedTemplatesList.TemplateSubject = _mappingService.ReplaceLabelsWithKeys( _selectedTemplatesList.TemplateSubject );
		//_selectedTemplatesList.TemplateContent = _mappingService.ReplaceLabelsWithKeys( _selectedTemplatesList.TemplateContent );


		// Haal dynamische data van recipients op
		_recipientData = await MailingService.GetDynamicRecipientsAsync( _currentRecipientQuery );

		if ( !_recipientData.Any() )
			return;

		_previewRecipients = [ .. _recipientData
			.Select( r => ( r as IDictionary<string, object> )? [ "Email" ]?.ToString() ?? string.Empty )
			.Where( e => !string.IsNullOrEmpty( e ) ) ];

		_currentPreviewIndex = 0;
		_selectedPreviewRecipient = _previewRecipients.First();

		if ( _recipientData?.Count > 0 )
		{
			SelectedRecipient = _recipientData [ 0 ];
			UpdatePreview( SelectedRecipient );
		}

		_showPreviewDialog = true;

		// Restore the dutch fieldnames in the UI
		_selectedTemplatesList.TemplateSubject = _tempSubject;
		_selectedTemplatesList.TemplateContent = _tempContent;
	}

	protected async Task MailTest()
	{
		_testEmailAddress = string.Empty;
		_testRecipientCount = 15;
		
		_showTestDialog = true;
	}


	protected async Task SendTestMail()
	{

        try
        {
            if ( _selectedTemplatesList?.RecipientListId == null )
                return;

            if ( string.IsNullOrWhiteSpace( _testEmailAddress ) )
            {
                // hier kun je eventueel een UI-melding tonen
                return;
            }

            _showTestDialog = false;

            _recipientData = await MailingService.GetDynamicRecipientsAsync( _currentRecipientQuery );

            await MailingService.SendTestMailAsync(
                _selectedTemplatesList,
                _recipientData,
                _testEmailAddress,
                _testRecipientCount
            );

            // Eventueel feedback aan gebruiker
            Debug.WriteLine( $"Testmail(s) verzonden naar {_testEmailAddress}" );
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Fout bij verzenden testmail: {ex.Message}" );
            // Als je IMailingLogger hebt, kun je hier ook loggen:
            _logger.LogError(ex, "Error sending test mail");
        }
    }

	protected async Task SendMailing()
	{
        try
        {
            if ( _selectedTemplatesList?.RecipientListId == null )
                return;

            _recipientData = await MailingService.GetDynamicRecipientsAsync( _currentRecipientQuery );

            await MailingService.SendBulkMailAsync(
                _selectedTemplatesList,
                _recipientData
            );

            Debug.WriteLine( "Bulkmailing verzonden." );
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Fout bij verzenden bulkmailing: {ex.Message}" );
        }
    }

    protected async Task MailSend()
    {
        await Task.CompletedTask;
    }

    private void UpdatePreview( ExpandoObject recipient )
	{
		if ( recipient is not IDictionary<string, object> data )
			return;

		// 1. Start met de originele template (NL labels)
		string subjectTemplate = _selectedTemplatesList.TemplateSubject ?? "";
		string bodyTemplate = _selectedTemplatesList.TemplateContent ?? "";

		// 2. Zet NL labels om naar interne keys die exact overeenkomen met database kolommen
		subjectTemplate = _mappingService.ReplaceLabelsWithKeys( subjectTemplate );
		bodyTemplate = _mappingService.ReplaceLabelsWithKeys( bodyTemplate );

		// 3. Vervang interne keys door waarden uit de database
		foreach ( var kvp in data )
		{
			string key = "{" + kvp.Key + "}"; // bv "{GroupName}", "{Firstname}"
			string value = kvp.Value?.ToString() ?? string.Empty;

			subjectTemplate = subjectTemplate.Replace( key, value, StringComparison.OrdinalIgnoreCase );
			bodyTemplate = bodyTemplate.Replace( key, value, StringComparison.OrdinalIgnoreCase );
		}

		_previewSubject = subjectTemplate;
		_previewBody = bodyTemplate;
	}

	private void OnRecipientChange( object? newValue )
	{
		if ( newValue is ExpandoObject recipient )
		{
			SelectedRecipient = recipient;
			_currentRecipientIndex = _recipientData.IndexOf( recipient );
			UpdatePreview( SelectedRecipient );
		}
	}

	private void FirstRecipient()
	{
		if ( _recipientData == null || !_recipientData.Any() )
			return;

		if ( _currentRecipientIndex > 0 )
			_currentRecipientIndex--;

		SelectedRecipient = _recipientData [ 0 ];
		UpdatePreview( SelectedRecipient );
	}

	private void PreviousRecipient()
	{
		if ( _recipientData == null || !_recipientData.Any() )
			return;

		if ( _currentRecipientIndex > 0 )
			_currentRecipientIndex--;

		SelectedRecipient = _recipientData [ _currentRecipientIndex ];
		UpdatePreview( SelectedRecipient );
	}

	private void NextRecipient()
	{
		if ( _recipientData == null || !_recipientData.Any() )
			return;

		if ( _currentRecipientIndex < _recipientData.Count - 1 )
			_currentRecipientIndex++;

		SelectedRecipient = _recipientData [ _currentRecipientIndex ];
		UpdatePreview( SelectedRecipient );
	}

	private void LastRecipient()
	{
		if ( _recipientData != null && _recipientData.Count > 0 )
		{
			SelectedRecipient = _recipientData [ ^1 ]; // ^1 = laatste element
			UpdatePreview( SelectedRecipient );
		}
	}

	private async Task UpdateRteSlashMenuAsync()
	{
		SlashMenuItems ??= [ ];

		var ready = await WaitForRteReadyAsync(maxAttempts: 15, delayMs: 100);
		if ( !ready )
		{
			Debug.WriteLine( "UpdateRteSlashMenuAsync: RTE not ready, aborting update." );
			return;
		}

		try
		{
			// Prepare JS items from the slash menu list
			var jsItems = SlashMenuItems
			.Select(s => new
			{
				text = s.Text,
				iconCss = s.IconCss,
				category = s.GroupBy
			})
			.ToArray();

			if ( jsItems.Length == 0 )
			{
				Debug.WriteLine( "UpdateRteSlashMenuAsync: no items available for the SlashMenu." );
				return;
			}

			// Call JS to refresh slash menu and context menu in one go
			await InvokeAsync( async () =>
			{
				try
				{
					// Update slash menu (as before)
					await JSRuntime.InvokeVoidAsync( "rteHelpers.updateSlashMenu", "rteContent", jsItems );
					await JSRuntime.InvokeVoidAsync( "rteHelpers.addInsertFieldHandler", "rteContent", jsItems );
					await JSRuntime.InvokeVoidAsync( "rteHelpers.updateContextMenu", "rteContent", SlashMenuItems.Select( s => new { text = s.Text, iconCss = s.IconCss } ) );

				}
				catch ( JSException jsEx )
				{
					Debug.WriteLine( $"[UpdateRteSlashMenuAsync] JS error: {jsEx.Message}" );
				}
			} );
		}
		catch ( TaskCanceledException )
		{
			// Ignored because we might switch templates quickly
		}
		catch ( ObjectDisposedException )
		{
			// Happens if user navigates away mid-update
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"[UpdateRteSlashMenuAsync] Unexpected: {ex.Message}" );
		}
	}

	private async Task OnTemplateChanged( uint? templateId )
	{
		if ( templateId != null && templateId != 0 )
		{
			SelectedTemplatesListId = templateId;
			await Task.Delay( 100 ); // Allow UI state to settle
			await UpdateRteSlashMenuAsync();
		}
	}

	private async Task OnRecipientListChanged( uint? recipientListId )
	{
		if ( recipientListId != null && recipientListId != 0 )
		{
			RecipientListId = recipientListId;
			await Task.Delay( 100 ); // Allow UI state to settle
			await UpdateRteSlashMenuAsync();
		}
	}

	private async Task<bool> WaitForRteReadyAsync( int maxAttempts = 10, int delayMs = 100 )
	{
		if ( JSRuntime is null ) return false;

		for ( int i = 0; i < maxAttempts; i++ )
		{
			try
			{
				bool ready = false;
				try
				{
					ready = await JSRuntime.InvokeAsync<bool>( "rteHelpers.isRteReady", "rteContent" );
				}
				catch ( Exception ex )
				{
					Debug.WriteLine( $"WaitForRteReadyAsync attempt {i + 1} failed: {ex.GetType().Name} - {ex.Message}" );
				}

				if ( ready )
					return true;
			}
			catch ( TaskCanceledException ) { return false; }
			catch ( ObjectDisposedException ) { return false; }

			await Task.Delay( delayMs );
		}

		Debug.WriteLine( "WaitForRteReadyAsync: RTE not ready after waiting." );
		return false;
	}

}