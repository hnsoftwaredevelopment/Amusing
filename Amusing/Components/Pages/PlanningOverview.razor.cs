using System.Xml.Linq;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using BootstrapBlazor.Components;

using ClosedXML.Excel;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Grids.Internal;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static Mysqlx.Expect.Open.Types;

namespace Amusing.Components.Pages;

public partial class PlanningOverview
{
    protected SfGrid<PlanningPerformancesModel> PerformancesGridRef;
    protected SfGrid<PlanningStageVolunteersModel> StageDutyGridRef;
    protected SfGrid<PlanningOtherVolunteerTasksModel> OtherTasksGridRef;
    protected List<Edition> Editions = [];
    [Inject] public PlanningService PlanningService { get; set; } = default!;
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    private string _message;
    private string SelectedEditionId { get; set; }
    public string SelectedEditionText { get; set; }

    public List<PlanningConditionsModel> Conditions { get; set; } = [];
    public PlanningConditionsModel? SelectedCondition { get; set; }
    public bool IsEditingConditions { get; set; } = false;

    public List<PlanningStageVolunteersModel> StageDuty { get; set; } = [ ];
    public PlanningStageVolunteersModel? SelectedStageDuty { get; set; }

    public List<PlanningOtherVolunteerTasksModel> OtherTasks { get; set; } = [ ];
    public PlanningOtherVolunteerTasksModel? SelectedOtherTasks { get; set; }

    public List<StageScheduleRow> StageRows = [];
    public List<string> TimeSlots = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        await LoadEditionsAsync();
        await LoadConditionsAsync();
        await LoadStageDutyAsync();
        await LoadOtherTasksAsync();
    }

    private async Task LoadEditionsAsync()
    {
        Editions = await EditionService.GetEditionsAsync();

        if ( Editions.Count != 0 )
        {
            // Auto select the newest festival by its numeric value
            SelectedEditionId = Editions
                .OrderByDescending( e => int.Parse( e.Text ) )
                .First().ID;

            // Find label text
            var edition = Editions.FirstOrDefault(e => e.ID == SelectedEditionId);
            SelectedEditionText = edition?.Text ?? "";
        }
    }

    private async Task LoadConditionsAsync()
    {
        if ( SelectedEditionId == null )
            return;

        Conditions = await PlanningService.GetPlanningConditionsAsync(
            int.Parse( SelectedEditionId )
        );

        // For your detail table, pick the first (often the only) record
        SelectedCondition = Conditions.FirstOrDefault();
    }

    private async Task LoadStageDutyAsync()
    {
        if ( SelectedEditionId == null )
            return;

        StageDuty = await PlanningService.GetPlanningVolunteersPerStageOverview(
            int.Parse( SelectedEditionId )
        );

        // For your detail table, pick the first (often the only) record
        SelectedStageDuty = StageDuty.FirstOrDefault();
    }

    private async Task LoadOtherTasksAsync()
    {
        if ( SelectedEditionId == null )
            return;

        OtherTasks = await PlanningService.GetPlanningOtherVolunteerTasksOverview(
            int.Parse( SelectedEditionId )
        );

        // For your detail table, pick the first (often the only) record
        SelectedOtherTasks = OtherTasks.FirstOrDefault();
    }


    protected async Task OnEditionChanged( string selectedId )
    {
        if ( string.IsNullOrWhiteSpace( selectedId ) )
            return;

        SelectedEditionId = selectedId;

        // Find label text
        var edition = Editions.FirstOrDefault(e => e.ID == selectedId);
        SelectedEditionText = edition?.Text ?? "";

        await LoadConditionsAsync();
        await LoadStageDutyAsync();
        await LoadOtherTasksAsync();

        TimeSlots = GenerateTimeSlots(); // optioneel opnieuw genereren
        var raw = await PlanningService.GetStagePerformancesAsync(int.Parse(SelectedEditionId));
        StageRows = BuildScheduleRows( raw );

        StateHasChanged();

    }

    private void ToggleEditConditions()
    {
        if ( IsEditingConditions )
        {
            // TODO: Save to database
            // Example:
            // await PlanningService.SaveConditionsAsync(SelectedCondition);
        }

        IsEditingConditions = !IsEditingConditions;
    }

    private async Task<string> ExportFilename (int _editionId, string fileType)
    {
        var ExportFileName = await PlanningService.GetExportFileName(_editionId);

        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string downloadsPath = Path.Combine(userProfile, "Downloads");

        string _fileName = Path.Combine(
            downloadsPath,
            $"{ExportFileName}.{fileType}"
        );
        return _fileName;
    }
    private async Task ExportToXmlAsync()
    {
        var _editionId = int.Parse( SelectedEditionId );
        var ExportFileName = ExportFilename(_editionId, "xml");
        // Trigger XML export for the selected edition
        await PlanningService.ExportFullPlanningToXmlAsync( _editionId, ExportFileName.ToString() );
        _message = "Planning naar XML export completed.";
    }

    private async Task ExportToExcelAsync()
    {
        var _editionId = int.Parse( SelectedEditionId );
        var ExportFileName = ExportFilename(_editionId, "xlxs");
        // Trigger Excel export for the selected edition
        await PlanningService.ExportFullPlanningToExcelAsync( _editionId, ExportFileName.ToString() );
        _message = "Planning naar Excel export completed.";
    }

    private PlanningStageVolunteersModel GetPreviousItem( PlanningStageVolunteersModel current )
    {
        var index = StageDuty.IndexOf(current);
        if ( index <= 0 )
            return null;

        return StageDuty [ index - 1 ];
    }

    private PlanningOtherVolunteerTasksModel GetPreviousTaskItem( PlanningOtherVolunteerTasksModel current )
    {
        var index = OtherTasks.IndexOf(current);
        if ( index <= 0 )
            return null;

        return OtherTasks [ index - 1 ];
    }

    protected override async Task OnParametersSetAsync()
    {
        // Generate the 30-min time headers
        TimeSlots = GenerateTimeSlots();

        // Fetch raw schedule data from the service
        var raw = await PlanningService.GetStagePerformancesAsync(int.Parse( SelectedEditionId));

        // Transform raw performances into pivot structure
        StageRows = BuildScheduleRows( raw );
    }

    // Generate times from 11:00 to 18:00 (30-minute steps)
    private List<string> GenerateTimeSlots()
    {
        var list = new List<string>();
        var time = new TimeOnly(11, 0);

        for ( int i = 0; i < 14; i++ ) // 7 hours * 2 slots/hour
        {
            list.Add( time.ToString( "HH:mm" ) );
            time = time.AddMinutes( 30 );
        }

        return list;
    }

    // Convert timeslot number to start time
    private TimeOnly SlotToStartTime( int tijdvak )
    {
        var baseTime = new TimeOnly(11, 0);
        return baseTime.AddMinutes( ( tijdvak - 1 ) * 30 );
    }

    // Pivot transformation
    private List<StageScheduleRow> BuildScheduleRows(List<StagePerformanceModel> data)
{
        var rows = data
        .GroupBy(x => new { x.StageId, x.StageName })
        .Select(g =>
        {
            var firstPerf = g.First(); // pak eerste performance om SortOrder te halen
            var row = new StageScheduleRow
            {
                StageId = g.Key.StageId,
                StageName = g.Key.StageName,
                SortOrder = firstPerf.SortOrder // <-- hier het nummer instellen
            };

            // Initialize all slots as empty
            foreach (var slot in TimeSlots)
                row[slot] = "";

            // Fill slots with group names
            foreach (var perf in g)
            {
                var key = SlotToStartTime(perf.Timeslot).ToString("HH:mm");
                row.SetSlot(key, perf.GroupName);
            }

            return row;
        })
        .OrderBy(x => x.SortOrder) // sorteren op het nieuwe Nr
        .ToList();

        return rows;
    }
}