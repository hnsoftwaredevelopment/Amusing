using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.QueryBuilder;

namespace Amusing.Components.Pages;

public class MailingsRecipientsBase : ComponentBase
{
    [Inject] protected MailingService MailingService { get; set; } = null!;
    [Inject] protected LoggingService LoggingService { get; set; } = null!;
    [Inject] protected NavigationManager NavManager { get; set; } = null!;
    [Inject] protected IJSRuntime JS { get; set; } = null!;
    [Inject] protected ToastService ToastService { get; set; } = null!;

    protected bool _initialLoadDone = false;
    protected bool FestivalSelected = false;
    protected bool HasActiveRules { get; set; }
    protected bool IsLoading = false;
    protected bool RoleSelected = false;
    protected bool rulesPending;
    protected string? pendingRulesJson;
    protected EditContext? editContext;
    protected int VisibleRowCount = 0;
    protected int SelectedListRowCount = 0;
    protected List<ExpandoObject> DynamicRecipients { get; set; } = new List<ExpandoObject>();
    protected List<ExpandoObject> DynamicExportRecipients { get; set; } = new List<ExpandoObject>();
    protected List<IDictionary<string, object>> DynamicRecipientsDict { get; set; } = new();
    protected List<RecipientListFilterModel> RecipientsFilterList { get; set; } = new List<RecipientListFilterModel>();
    protected List<RecipientListModel> RecipientsList { get; set; } = [];
    protected List<string> FestivalValues { get; set; } = new();
    protected List<string> RoleValues { get; set; } = new();
    protected List<OperatorsModel> EqualOnlyOperator = new()
        {
            new OperatorsModel { Value = "equal", Text = "Is gelijk aan" }
        };
    protected List<OperatorsModel> NumberOperator = new()
        {
            new OperatorsModel { Value="equal", Text = "Gelijk aan" },
            new OperatorsModel { Value="notequal", Text = "Niet gelijk aan" },
            new OperatorsModel { Value="greaterthan", Text = "Groter dan" },
            new OperatorsModel { Value="greaterthanorequal", Text = "Groter of gelijk aan" },
            new OperatorsModel { Value="lessthan", Text = "Kleiner dan" },
            new OperatorsModel { Value="lessthanorequal", Text = "Kleiner dan of gelijk aan" },
        };
    protected RecipientListModel? SelectedRecipientsList;
    protected SfGrid<RecipientListFilterModel>? GridRefReceipts;
    protected SfGrid<RecipientListModel>? GridRef;
    protected SfGrid<ExpandoObject>? SelectedListGridRef;
    protected SfGrid<ExpandoObject>? ExportGridRef;
    protected SfQueryBuilder<RuleModel>? personsQueryBuilder;
    protected SfQueryBuilder<RuleModel>? groupsQueryBuilder;
    protected SfToast? ToastObj { get; set; }
    protected List<string> PersonRoles = ["contactpersoon1", "contactpersoon2", "dirigent", "muzikant", "penningmeester", "zanger"];
    protected static readonly string[] InFields = { "Festival", "Role", "Volunteer" };
    protected string PageName = "Mailing lijsten";
    protected string FileName { get; set; } = "";
    protected string GeneratedSql = string.Empty;
    protected string SourceChecked = "groups";
    protected string[] YesNoValues = ["Ja", "Nee"];
    protected string GeneratedJson = string.Empty;
    protected string OldJson = string.Empty;
    protected RenderFragment<bool?> BooleanTemplate => (value) => builder => builder.AddContent(0, value == true ? "Ja" : "Nee");
    protected string[] LastSelectedFestivals = Array.Empty<string>(); // For autoselect the Festivals for Volunteers
    protected static readonly List<ExportColumnDefinition> ExportColumns =
    [
        new("Name", "Volledige naam"),
        new("GroupName", "Groep"),
        new("Email", "E-mail"),
        new("Firstname", "Voornaam"),
        new("Lastname", "Achternaam"),
        new("Infomailing", "Infomailing"),
        new("Active", "Aktief"),
        new("Role", "Rol"),
        new("Festival", "Editie"),
        new("StageType", "Podiumtype"),
        new("Subscribed", "Ingeschreven"),
        new("Canceled", "Afgehaakt"),
        new("Paid", "Betaald"),
        new("Confirmed", "Bevestigd"),
        new("Dressingroom", "Kleedkamer"),
        new("SingAlong", "SingAlong"),
        new("AcapellaBattle", "AcapellaBattle"),
        new("Judgement", "Beoordeling"),
        new("Singers", "Zangers"),
        new("Volunteer", "Vrijwilliger")
    ];

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        //Get all recipientlists
        RecipientsList = await MailingService.GetRecipientListsAsync();
        SelectedRecipientsList = RecipientsList.FirstOrDefault();
        if (SelectedRecipientsList != null)
        {
            editContext = new EditContext(SelectedRecipientsList);
        }

        // Get Festival values
        FestivalValues = await MailingService.GetFestivalListAsync();

        if (SelectedRecipientsList != null)
        {
            if (string.IsNullOrWhiteSpace(SelectedRecipientsList.ListQuery) && !string.IsNullOrWhiteSpace(SelectedRecipientsList.ListFilter))
            {
                SelectedRecipientsList.ListQuery = QueryBuilderJsonConverter.OldToNew(SelectedRecipientsList.ListFilter);
            }

            LoadRulesIntoQueryBuilder(SelectedRecipientsList.ListQuery);
        }

        IsLoading = false;
    }

    public string SelectedRecipientsListName
    {
        get => SelectedRecipientsList?.ListName ?? "geen ontvangerslijst beschikbaar";
        set
        {
            if (SelectedRecipientsList != null)
                SelectedRecipientsList.ListName = value;
        }
    }

    protected async Task OnGridDataBound()
    {
        if (!_initialLoadDone && RecipientsList?.Any() == true)
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();

            // Select first row in the SfGrid
            if (GridRef != null)
            {
                await GridRef.SelectRowAsync(0);
            }

            // filll QueryBuilder with query data from selected row
            if (RecipientsList.Count > 0)
            {
                await SelectRecipientListAsync(RecipientsList[0]);
            }
        }
    }

    protected async Task OnRowSelected(RowSelectEventArgs<RecipientListModel> args)
    {
        await SelectRecipientListAsync(args.Data);
    }

    protected async Task SelectRecipientListAsync(RecipientListModel recipient)
    {
        if (recipient == null)
        {
            return;
        }

        SelectedRecipientsList = recipient;
        editContext = new EditContext(SelectedRecipientsList);

        SourceChecked = SelectedRecipientsList.ListSource switch
        {
            MailingService.RecipientListSource.Groups => "groups",
            MailingService.RecipientListSource.Persons => "persons",
            _ => string.Empty
        };

        if (personsQueryBuilder != null)
        {
            string? json = SelectedRecipientsList.ListQuery;

            // Alleen converteren als er geen nieuwe JSON is
            if (string.IsNullOrWhiteSpace(json) && !string.IsNullOrWhiteSpace(SelectedRecipientsList.ListFilter))
            {
                json = QueryBuilderJsonConverter.OldToNew(SelectedRecipientsList.ListFilter);
                SelectedRecipientsList.ListQuery = json;
            }

            // Zet de regels in de QueryBuilder
            await LoadRulesAsync(json);

            // Fix alle "in"-velden die null zijn
            //FixInFields( personsQueryBuilder.GetRules()?.Rules );

            // Eventueel: subscribeer op nieuwe regels zodat sanity check automatisch blijft gelden
            //queryBuilder.RuleAdded += ( sender, args ) => FixInFields( new [ ] { args.Rule } );
        }

        await GenerateMailList();
        await InvokeAsync(StateHasChanged);
    }

    #region FixInFields
    protected void FixInFields(RuleModel? rule)
    {
        if (rule == null)
        {
            return;
        }

        if (InFields.Contains(rule.Field) && rule.Operator == "in" && rule.Value == null)
        {
            rule.Value = Array.Empty<string>();
        }

        //if ( rule.Rules != null && rule.Rules.Any() )
        //{
        //    FixInFields( rule.Rules );
        //}
    }

    protected void FixInFields(IEnumerable<RuleModel>? rules)
    {
        if (rules == null)
        {
            return;
        }

        //foreach ( RuleModel r in rules )
        //{
        //    FixInFields( r );
        //}
    }
    #endregion

    protected void LoadRulesIntoQueryBuilder(string? json)
    {
        if (personsQueryBuilder == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            personsQueryBuilder.Reset();
            return;
        }

        List<RuleModel> rules = QueryBuilderJsonConverter.ToRuleModels(json);
        personsQueryBuilder.SetRules(rules);

        // Force AND condition en fix alle "in"-velden
        ForceAndCondition(rules);
        //FixInFields( rules );
    }

    #region ForceAndCondition
    protected void ForceAndCondition(RuleModel? rule)
    {
        if (rule == null)
        {
            return;
        }

        if (rule.Rules != null && rule.Rules.Any())
        {
            rule.Condition = "and";
            // recurse into children
            ForceAndCondition(rule.Rules);
        }
        else
        {
            rule.Condition = null;
        }
    }

    protected void ForceAndCondition(IEnumerable<RuleModel>? rules)
    {
        if (rules == null)
        {
            return;
        }

        foreach (RuleModel r in rules)
        {
            ForceAndCondition(r);
        }
    }
    #endregion

    protected static RuleModel BuildRuleFromJsonElement(JsonElement elem)
    {
        RuleModel rule = new();

        // Strings / eenvoudige velden
        if (elem.TryGetProperty("Field", out JsonElement pField) && pField.ValueKind == JsonValueKind.String)
        {
            rule.Field = pField.GetString();
        }

        if (elem.TryGetProperty("Label", out JsonElement pLabel) && pLabel.ValueKind == JsonValueKind.String)
        {
            rule.Label = pLabel.GetString();
        }

        if (elem.TryGetProperty("Operator", out JsonElement pOperator))
        {
            rule.Operator = pOperator.ValueKind == JsonValueKind.String ? pOperator.GetString() : pOperator.ToString();
        }

        if (elem.TryGetProperty("Type", out JsonElement pType) && pType.ValueKind == JsonValueKind.String)
        {
            rule.Type = pType.GetString();
        }

        if (elem.TryGetProperty("RuleId", out JsonElement pRuleId) && pRuleId.ValueKind == JsonValueKind.String)
        {
            rule.RuleId = pRuleId.GetString();
        }

        // Booleans
        if (elem.TryGetProperty("IsLocked", out JsonElement pIsLocked) && pIsLocked.ValueKind != JsonValueKind.Null)
        {
            if (pIsLocked.ValueKind == JsonValueKind.True)
            {
                rule.IsLocked = true;
            }
            else if (pIsLocked.ValueKind == JsonValueKind.False)
            {
                rule.IsLocked = false;
            }
        }
        if (elem.TryGetProperty("Not", out JsonElement pNot) && pNot.ValueKind != JsonValueKind.Null)
        {
            if (pNot.ValueKind == JsonValueKind.True)
            {
                rule.Not = true;
            }
            else if (pNot.ValueKind == JsonValueKind.False)
            {
                rule.Not = false;
            }
        }
        if (elem.TryGetProperty("Condition", out JsonElement pCond) && pCond.ValueKind == JsonValueKind.String)
        {
            rule.Condition = pCond.GetString();
        }

        // Value: kan Array of Single Value zijn
        if (elem.TryGetProperty("Value", out JsonElement pValue))
        {
            // For convenience treat Operator as string now
            dynamic? op = rule.Operator?.ToString();

            if (pValue.ValueKind == JsonValueKind.Array)
            {
                List<object?> items = pValue.EnumerateArray()
                                  .Select(e => ConvertJsonElement(e, rule.Type))
                                  .ToList();

                if (string.Equals(op, "in", StringComparison.OrdinalIgnoreCase))
                {
                    rule.Value = rule.Type?.ToLower() switch
                    {
                        "string" => items.Select(v => v?.ToString() ?? string.Empty).ToArray(),
                        "boolean" => items.Select(v => Convert.ToBoolean(v)).ToArray(),
                        "number" or "int" or "double" => items.Select(v => Convert.ToDouble(v)).ToArray(),
                        _ => items.ToArray()
                    };
                }
                else
                {
                    rule.Value = items.ToArray();
                }
            }
            else
            {
                // single Value
                object? single = ConvertJsonElement(pValue, rule.Type);

                if (string.Equals(op, "in", StringComparison.OrdinalIgnoreCase))
                {
                    // force array for 'in'
                    if (single == null)
                    {
                        rule.Value = Array.Empty<string>();
                    }
                    else if (single is string s)
                    {
                        rule.Value = new string[] { s };
                    }
                    else if (single is bool b)
                    {
                        rule.Value = new bool[] { b };
                    }
                    else if (single is double d)
                    {
                        rule.Value = new double[] { d };
                    }
                    else
                    {
                        rule.Value = new object[] { single };
                    }
                }
                else
                {
                    rule.Value = single;
                }
            }
        }

        // Recursief: child rules (Rules)
        if (elem.TryGetProperty("Rules", out JsonElement pRules) && pRules.ValueKind == JsonValueKind.Array)
        {
            List<RuleModel> list = new();
            foreach (JsonElement child in pRules.EnumerateArray())
            {
                list.Add(BuildRuleFromJsonElement(child));
            }

            rule.Rules = list;
        }

        return rule;
    }

    protected async Task LoadRulesAsync(string? json)
    {
        if (personsQueryBuilder is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            personsQueryBuilder.Reset();
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement rootElem = doc.RootElement;

            RuleModel rootRule = BuildRuleFromJsonElement(rootElem);

            // Force AND en fix children
            ForceAndCondition(rootRule);

            if (rootRule.Rules != null && rootRule.Rules.Any())
            {
                personsQueryBuilder.SetRules(rootRule.Rules);
            }
            else
            {
                personsQueryBuilder.SetRules(new List<RuleModel> { rootRule });
            }

            return;
        }
        catch
        {
            personsQueryBuilder.Reset();
        }

        await InvokeAsync(StateHasChanged);
    }

    protected static readonly HashSet<string> BooleanFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "IsCanceled", "IsPaid", "Dressingroom", "Jury", "Infomailing", "IsConfirmed", "IsSubscribed"
        };

    protected static object? ConvertJsonElement(JsonElement element, string? type)
    {
        if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return type?.ToLower() switch
        {
            "boolean" => element.GetBoolean(),
            "number" or "double" or "int" => element.TryGetDouble(out double d) ? d : 0,
            "string" => element.GetString(),
            _ => element.ToString() // fallback
        };
    }

    protected static object? ConvertJsonElementToClr(JsonElement elem, string? fieldName)
    {
        switch (elem.ValueKind)
        {
            case JsonValueKind.True:
            case JsonValueKind.False:
                return elem.GetBoolean();

            case JsonValueKind.Number:
                // If the Field is a boolean Field (older data might save 0/1) => convert to bool
                if (!string.IsNullOrEmpty(fieldName) && BooleanFields.Contains(fieldName))
                {
                    int n = 0;
                    if (elem.TryGetInt32(out n))
                    {
                        return n != 0;
                    }

                    if (elem.TryGetInt64(out long l))
                    {
                        return l != 0L;
                    }
                }

                // Otherwise return integer
                if (elem.TryGetInt32(out int i))
                {
                    return i;
                }

                if (elem.TryGetInt64(out long l2))
                {
                    return l2;
                }

                if (elem.TryGetDouble(out double d))
                {
                    return d;
                }

                return elem.GetRawText();

            case JsonValueKind.String:
                string s = elem.GetString() ?? string.Empty;
                // if string contains comma and Operator was 'in' earlier, we may want an array,
                // but decide later from RuleModel.Operator. For now return string.
                // Some callers expect arrays, but FixRuleValues will also run on the parent to convert if needed.
                // Convert "0"/"1" to bool for boolean fields
                if (!string.IsNullOrEmpty(fieldName) && BooleanFields.Contains(fieldName))
                {
                    if (s == "0")
                    {
                        return false;
                    }

                    if (s == "1")
                    {
                        return true;
                    }

                    if (bool.TryParse(s, out bool bv))
                    {
                        return bv;
                    }
                }
                return s;

            case JsonValueKind.Array:
                // convert all array items to strings (most common for 'in')
                string[] items = elem.EnumerateArray()
                        .Select(x => x.ValueKind == JsonValueKind.String ? x.GetString() ?? string.Empty : x.ToString())
                        .Where(x => x != null)
                        .ToArray();
                return items;

            default:
                return elem.GetRawText();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if (GridRef is not null)
        {
            List<RecipientListModel> records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay(150);
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    public async Task OnInput(InputEventArgs args)
    {
        if (GridRef != null)
        {
            await GridRef.SearchAsync(args.Value);
            await Task.Delay(50);
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task Save()
    {
        if (SelectedRecipientsList is null)
        {
            return;
        }

        RuleModel? rules = personsQueryBuilder?.GetRules();

        // Force AND
        ForceAndCondition(rules);

        SelectedRecipientsList.ListQuery = JsonSerializer.Serialize(rules, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        if (SelectedRecipientsList.ListId != 0)
        {
            await MailingService.UpdateRecipientQueryAsync(SelectedRecipientsList);
            string logMessage = $"<_userName> heeft de ontvangerslijst {SelectedRecipientsList.ListName} aangepast.";
            await LoggingService.WriteUserActionRecipientListAsync( SelectedRecipientsList.ListId, "Mailing", "Ontvangerslijsten", "success", logMessage );
            await ToastService.ShowSuccessAsync( $"De wijzigingen voor ontvangerslijst {SelectedRecipientsList.ListName} zijn opgeslagen." );
        }
        else
        {
            // Save the new group and get the new group Id
            var savedId = await MailingService.AddRecipientQueryAsync(SelectedRecipientsList);

            // Refresh the list
            RecipientsList = await MailingService.GetRecipientListsAsync();
            await Task.Delay(50);
            if (GridRef != null)
            {
                await GridRef.Refresh();
            }

            // Search the modified record
            var index = RecipientsList.FindIndex(s => s.ListId == savedId);
            if (index >= 0)
            {
                SelectedRecipientsList = RecipientsList[index];
                await GridRef.SelectRowAsync(index);
            }

            string logMessage = $"<_userName> heeft de ontvangerslijst {SelectedRecipientsList?.ListName} toegevoegd.";
            await LoggingService.WriteUserActionRecipientListAsync( savedId, "Mailing", "Ontvangerslijsten", "success", logMessage );
            await ToastService.ShowSuccessAsync( $"Ontvangerslijst {SelectedRecipientsList?.ListName} is opgeslagen." );
        }
    }

    protected async Task AddNew()
    {
        RecipientListModel newRecipientsList = new()
        {
            ListId = 0,
            ListName = ""
        };

        if (GridRef != null)
        {
            await GridRef.AddRecordAsync(newRecipientsList, 0);
            await GridRef.SelectRowAsync(0);
        }

        SelectedRecipientsList = newRecipientsList;
        editContext = new EditContext(SelectedRecipientsList);
        StateHasChanged();
    }

    protected async Task Delete()
    {
        var list = SelectedRecipientsList;
        if (list == null || list.ListId == 0)
        {
            return;
        }

        await MailingService.DeleteRecipientQueryAsync((uint)list.ListId);
        string logMessage = $"<_userName> heeft de ontvangerslijst {list.ListName} verwijderd.";
        await LoggingService.WriteUserActionRecipientListAsync( list.ListId, "Mailing", "Ontvangerslijsten", "success", logMessage );
        await ToastService.ShowSuccessAsync( $"Ontvangerslijst {list.ListName} is verwijderd." );

        // Refresh the list
        RecipientsList = await MailingService.GetRecipientListsAsync();
        await Task.Delay(50);
        if (GridRef != null)
        {
            await GridRef.Refresh();
        }
    }

    protected void OnSourceChange(Microsoft.AspNetCore.Components.ChangeEventArgs args)
    {
        string value = args.Value?.ToString() ?? string.Empty;

        SelectedRecipientsList!.ListSource = value switch
        {
            "groups" => MailingService.RecipientListSource.Groups,
            "persons" => MailingService.RecipientListSource.Persons,
            _ => MailingService.RecipientListSource.Unknown
        };
    }

    protected void OnOueryChanged(Syncfusion.Blazor.QueryBuilder.ChangeEventArgs args)
    {
        RuleModel? rootRules = personsQueryBuilder?.GetRules();

        if (rootRules == null || rootRules.Rules == null || !rootRules.Rules.Any())
        {
            // Query nog niet compleet, niks doen
            GeneratedJson = "Nog geen complete query...";
            GeneratedJson = "Nog geen complete query...";
            HasActiveRules = false;
            return;
        }

        foreach (RuleModel? rule in rootRules.Rules)
        {
            if ((rule.Field == "Festival" || rule.Field == "Role" || rule.Field == "Volunteer")
                && rule.Operator == "in")
            {
                switch (rule.Value)
                {
                    case string s:
                        // convert single string into an array
                        rule.Value = new string[] { s };
                        break;

                    case null:
                        // ensure we have an empty array instead of null
                        rule.Value = Array.Empty<string>();
                        break;
                }
            }
        }

        try
        {
            // Serialize to JSON
            GeneratedJson = JsonSerializer.Serialize(rootRules, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            GeneratedJson = $"Error: {ex.Message}";
        }

        //Alleen boolean gebruiken om te bepalen of festival geselecteerd is
        HasActiveRules = rootRules.Rules.Count != 0;
        FestivalSelected = rootRules.Rules.Any(r => r.Field == "Festival" && r.Value is string[] arr && arr.Length > 0);
    }

    protected void ChangeValue(MultiSelectChangeEventArgs<string[]> args, RuleModel rule)
    {
        // Zorg dat de rule.Value exact de array bevat die door de MultiSelect wordt geleverd
        rule.Value = args.Value ?? Array.Empty<string>();
        // (optioneel) forceer UI update
        StateHasChanged();
    }

    #region Export related functions
    protected async Task ExportToExcel()
    {
        if (!await PrepareExport())
        {
            return;
        }

        ExcelExportProperties exportProps = new()
        {
            FileName = $"{PageName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        try
        {
            await ToastService.ShowExportStartedAsync(exportProps.FileName);
            if (ExportGridRef != null)
            {
                await ExportGridRef.ExportToExcelAsync(exportProps);
            }

            await ToastService.ShowExportCompletedAsync(exportProps.FileName, "Excel");
            await LogRecipientListExportAsync( "Excel", exportProps.FileName, "success" );
        }
        catch (Exception ex)
        {
            await ShowToast($"Export naar Excel mislukt: {ex.Message}", "error");
            await LogRecipientListExportAsync( "Excel", exportProps.FileName, "unsuccessful" );
        }
    }

    protected async Task ExportToCsv()
    {
        if (!await PrepareExport())
        {
            return;
        }

        ExcelExportProperties exportProps = new()
        {
            FileName = $"{PageName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        try
        {
            await ToastService.ShowExportStartedAsync(exportProps.FileName);
            if (ExportGridRef != null)
            {
                await ExportGridRef.ExportToCsvAsync(exportProps);
            }

            await ToastService.ShowExportCompletedAsync(exportProps.FileName, "CSV");
            await LogRecipientListExportAsync( "CSV", exportProps.FileName, "success" );
        }
        catch (Exception ex)
        {
            await ShowToast($"Export naar CSF mislukt: {ex.Message}", "error");
            await LogRecipientListExportAsync( "CSV", exportProps.FileName, "unsuccessful" );
        }
    }

    protected async Task ExportToPdf()
    {
        if (!await PrepareExport())
        {
            return;
        }

        PdfExportProperties exportProps = new()
        {
            FileName = $"{PageName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize = PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };

        try
        {
            await ToastService.ShowExportStartedAsync(exportProps.FileName);
            if (ExportGridRef != null)
            {
                await ExportGridRef.ExportToPdfAsync(exportProps);
            }

            await ToastService.ShowExportCompletedAsync(exportProps.FileName, "PDF");
            await LogRecipientListExportAsync( "PDF", exportProps.FileName, "success" );
        }
        catch (Exception ex)
        {
            await ShowToast($"Export naar PDF mislukt: {ex.Message}", "error");
            await LogRecipientListExportAsync( "PDF", exportProps.FileName, "unsuccessful" );
        }
    }

    protected async Task<bool> PrepareExport()
    {
        await GenerateMailList();

        if (ExportGridRef == null || DynamicExportRecipients == null || DynamicExportRecipients.Count == 0)
        {
            await ShowToast("Geen gegevens beschikbaar om te exporteren.", "warning");
            return false;
        }

        return true;
    }

    private async Task LogRecipientListExportAsync( string format, string fileName, string status )
    {
        string listName = SelectedRecipientsList?.ListName ?? "onbekende ontvangerslijst";
        string logMessage = status == "success"
            ? $"<_userName> heeft de ontvangerslijst {listName} geexporteerd naar {format} ({fileName})."
            : $"<_userName> kon de ontvangerslijst {listName} niet exporteren naar {format} ({fileName}).";

        if ( SelectedRecipientsList?.ListId > 0 )
        {
            await LoggingService.WriteUserActionRecipientListAsync( SelectedRecipientsList.ListId, "Mailing", "Ontvangerslijsten", status, logMessage );
            return;
        }

        await LoggingService.WriteUserActionAsync( "Mailing", "Ontvangerslijsten", status, logMessage );
    }

    protected async Task GenerateMailList()
    {
        RuleModel? rules = personsQueryBuilder?.GetRules();
        if (rules == null)
        {
            DynamicRecipients = [];
            DynamicExportRecipients = [];
            SelectedListRowCount = 0;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ForceAndCondition(rules);

        string fullQuery = QueryBuilderHelper.DetermineQueryFromRules(rules, SourceChecked);
        List<ExpandoObject> result = await MailingService.GetDynamicRecipientsAsync(fullQuery) ?? [];

        HashSet<string> jaNeeVelden = new(
        [
            "Infomailing", "Active","Subscribed","Canceled","Paid","Confirmed",
            "Dressingroom","SingAlong","Stand","Judgement","Volunteer"
        ], StringComparer.OrdinalIgnoreCase);


        // Convert values to Ja/Nee
        foreach (var row in result)
        {
            var dict = row as IDictionary<string, object>;

            foreach (var key in jaNeeVelden)
            {
                if (!dict.TryGetValue(key, out var val))
                    continue;

                // Convert bool directly
                if (val is bool b)
                {
                    dict[key] = b ? "Ja" : "Nee";
                }
                else
                {
                    var tempVal = val?.ToString();

                    if (tempVal == "0" || tempVal == "1")
                    {
                        dict[key] = tempVal == "1" ? "Ja" : "Nee";
                    }
                }

            }
        }

        DynamicRecipients = result.Select(ToSelectedListRecipient).ToList();
        DynamicExportRecipients = result.Select(ToExportRecipient).ToList();
        SelectedListRowCount = DynamicRecipients.Count;
        await InvokeAsync(StateHasChanged);
    }

    protected static ExpandoObject ToSelectedListRecipient(ExpandoObject row)
    {
        var source = (IDictionary<string, object?>)row;
        IDictionary<string, object?> recipient = new ExpandoObject();

        recipient["Name"] = GetFirstValue(source, "Name", "FullName") ?? string.Empty;
        recipient["GroupName"] = GetFirstValue(source, "GroupName") ?? string.Empty;
        recipient["Email"] = GetFirstValue(source, "Email", "PersonEmail", "GroupEmail") ?? string.Empty;

        return (ExpandoObject)recipient;
    }

    protected static ExpandoObject ToExportRecipient(ExpandoObject row)
    {
        var source = (IDictionary<string, object?>)row;
        IDictionary<string, object?> recipient = new ExpandoObject();

        recipient["Name"] = GetFirstValue(source, "Name", "FullName") ?? string.Empty;
        recipient["GroupName"] = GetFirstValue(source, "GroupName") ?? string.Empty;
        recipient["Email"] = GetFirstValue(source, "Email", "PersonEmail", "GroupEmail") ?? string.Empty;
        recipient["Firstname"] = GetFirstValue(source, "Firstname", "FirstName") ?? string.Empty;
        recipient["Lastname"] = GetFirstValue(source, "Lastname", "LastName") ?? string.Empty;
        recipient["Infomailing"] = GetFirstValue(source, "Infomailing") ?? string.Empty;
        recipient["Active"] = GetFirstValue(source, "Active") ?? string.Empty;
        recipient["Role"] = GetFirstValue(source, "Role", "ROLE") ?? string.Empty;
        recipient["Festival"] = GetFirstValue(source, "Festival") ?? string.Empty;
        recipient["StageType"] = GetFirstValue(source, "StageType") ?? string.Empty;
        recipient["Subscribed"] = GetFirstValue(source, "Subscribed") ?? string.Empty;
        recipient["Canceled"] = GetFirstValue(source, "Canceled") ?? string.Empty;
        recipient["Paid"] = GetFirstValue(source, "Paid") ?? string.Empty;
        recipient["Confirmed"] = GetFirstValue(source, "Confirmed") ?? string.Empty;
        recipient["Dressingroom"] = GetFirstValue(source, "Dressingroom", "Kleedkamer") ?? string.Empty;
        recipient["SingAlong"] = GetFirstValue(source, "SingAlong") ?? string.Empty;
        recipient["AcapellaBattle"] = GetFirstValue(source, "AcapellaBattle") ?? string.Empty;
        recipient["Judgement"] = GetFirstValue(source, "Judgement", "Beoordeling") ?? string.Empty;
        recipient["Singers"] = GetFirstValue(source, "Singers") ?? string.Empty;
        recipient["Volunteer"] = GetFirstValue(source, "Volunteer", "Vrijwilliger") ?? string.Empty;

        return (ExpandoObject)recipient;
    }

    protected static object? GetFirstValue(IDictionary<string, object?> row, params string[] fields)
    {
        foreach (string field in fields)
        {
            if (row.TryGetValue(field, out object? value) && value != null)
            {
                return value;
            }
        }

        return null;
    }

    protected string TranslateToDutch(string field)
    {
        return field switch
        {
            "Firstname" => "Voornaam",
            "Lastname" => "Achternaam",
            "Name" => "Volledige naam",
            "Email" => "E-mail",
            "Role" => "Rol",
            "ROLE" => "Rol",
            "GroupName" => "Groep",
            "Festival" => "Editie",
            "StageType" => "Podiumtype",
            "Dressingroom" => "Kleedkamer",
            "Judgement" => "Beoordeling",
            "Volunteer" => "Vrijwilliger",
            "Active" => "Aktief",
            "Subscribed" => "Ingeschreven",
            "Canceled" => "Afgehaakt",
            "Paid" => "Betaald",
            "Confirmed" => "Bevestigd",
            "Singers" => "Zangers",
            _ => field
        };
    }

    protected async Task ShowToast(string message, string type = "error")
    {
        string css = type switch
        {
            "success" => "e-toast-success",
            "warning" => "e-toast-warning",
            _ => "e-toast-danger"
        };

        string icon = type switch
        {
            "success" => "e-check",
            "warning" => "e-warning",
            _ => "e-error"
        };

        if (ToastObj != null)
        {
            await ToastObj.ShowAsync(new ToastModel
            {
                Title = "Export",
                Content = message,
                CssClass = css,
                Icon = icon
            });
        }
    }

    protected void OnToastClose(ToastCloseArgs args)
    {
        // Do something after the toast cloases
    }
    #endregion

    protected record ExportColumnDefinition(string Field, string HeaderText);
}
