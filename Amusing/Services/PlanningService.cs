using System.Data;
using System.Xml.Linq;

using Amusing.DataReaderExtensions;
using Amusing.Helpers;
using Amusing.Models;

using ClosedXML.Excel;

using GetMyString = Amusing.DataReaderExtensions.ReaderExtensions;

namespace Amusing.Services;

public class PlanningService ( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    #region Export FileName
    public async Task<string> GetFileName( int festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        var result = await _dataService.ExecuteQueryAsync(QueryDefinitions.PlanningExportFilename,
            reader => reader.GetString("FileName"),
        parameters);

        return result.ToString();
    }    #endregion

    #region Festivals
    public Task<List<PlanningFestivalsModel>> GetPlanningFestivalsAsync( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningFestivals,
           reader => new PlanningFestivalsModel
           {
               FestivalId = reader.GetMyUInt( "FestivalId" ),
               Festival = $"Amusing Hengelo {reader.GetMyString ( "Festival" )}",
               PerformanceLength = 30,
               StartFestivalday = reader.GetMyTime ( "StartFestivalday" ) ,
               EndFestivalday = reader.GetMyTime ( "EndFestivalday" ),
               StartPause = reader.GetMyTime ("StartPause" ),
               EndPause = reader.GetMyTime ( "EndPause" ),
               EndExperiencedSubstitude = reader.GetMyTime ( "EndExperiencedSubstitude" )
           }, parameters );
    }
    #endregion

    #region Genres
    public Task<List<PlanningGenresModel>> GetPlanningGenresAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningGenres,
            reader => new PlanningGenresModel
            {
                GenreId = reader.GetMyInt ( "GenreId" ),
                Name = reader.GetMyString ("Name" )
            } );
    }
    #endregion

    #region Groups
    public Task<List<PlanningGroupsModel>> GetPlanningGroupsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningGroups,
            reader => new PlanningGroupsModel
            {
                GroupId = reader.GetMyUInt ( "GroupId" ),
                Name = reader.GetMyString ( "Name" ),
                GenreId = reader.GetMyUInt ( "GenreId" ),
                City = reader.GetMyString ( "City" ),
                Country = reader.GetMyString ( "Country" )
            } );
    }
    #endregion

    #region Performances
    public async Task<bool> HasPerformances( int festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        var result = await _dataService.ExecuteQueryAsync(QueryDefinitions.HasPlanningPerformances, 
            reader => reader.GetBoolean(reader.GetOrdinal("HasRows")),
        parameters);

        return result.FirstOrDefault( false );
    }

    public Task<List<PlanningPerformancesModel>> GetPlanningPerformancesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningPerformances,
        reader =>
        {
            var stageName = reader.GetMyString ("StageName");
            var fromTime = reader.GetMyTime ("From");
            var groupName = reader.GetMyString ( "GroupName");

            return new PlanningPerformancesModel
            {
                FestivalId = reader.GetMyUInt ( "FestivalId" ),
                GroupId = reader.GetMyUInt( "GroupId" ),
                GroupName = groupName,
                TimeSlotId = reader.GetMyUInt( "TimeSlotId" ),
                StageId = reader.GetMyUInt( "StageId" ),
                StageName = stageName,
                From = fromTime,
                To = reader.GetMyTime ( "To" ),
                Pinned = false,
                Description = $"{stageName}, starttijd: {fromTime:hh\\:mm}, zanggroep: {groupName}"
            };
        } );
    }
    #endregion

    #region Person Roles
    public Task<List<PlanningPersonRolesModel>> GetPlanningPersonRolesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningPersonRoles,
            reader => new PlanningPersonRolesModel
            {
                PersonId = reader.GetMyUInt ( "PersonId" ),
                PersonName = reader.GetMyString ( "PersonName" ),
                GroupId = reader.GetMyInt ( "GroupId" ),
                GroupName = reader.GetMyString ( "GroupName" ),
                Role = reader.GetMyString ( "Role" )
            } );
    }
    #endregion

    #region Persons
    public Task<List<PlanningPersonsModel>> GetPlanningPersonsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningPersons,
            reader => new PlanningPersonsModel
            {
                PersonId = reader.GetMyUInt ( "PersonId" ),
                FirstName = reader.GetMyString ( "FirstName" ),
                Affix = reader.GetMyString ( "Affix" ),
                Surname = reader.GetMyString ( "Surname" ),
                Name = reader.GetMyString ( "Name" )
            } );
    }
    #endregion

    #region Registrations
    public Task<List<PlanningRegistrationsModel>> GetPlanningRegistrationsAsync(int _festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningRegistrations,
            reader => new PlanningRegistrationsModel
            {
                FestivalId = reader.GetMyUInt ( "FestivalId" ),
                GroupId = reader.GetMyUInt( "GroupId" ),
                GroupName = reader.GetMyString ( "GroupName" ),
                Wish1 = reader.GetMyString ( "Wish1" ),
                Wish2 = reader.GetMyString ( "Wish2" ),
                Wish3 = reader.GetMyString ( "Wish3" ),
                Wish4 = reader.GetMyString ( "Wish4" ),
                Singers = reader.GetMyUInt ( "Singers" ),
                Stagetype = reader.GetMyString ( "Stagetype" ),
                ForcedStageChoice = reader.GetMyInt ( "ForcedStageChoice" ),
                Registered = reader.GetMyDateTime ( "Registered" ),
                AvailableFrom = reader.GetMyTime ( "AvailableFrom" ),
                AvailableTill = reader.GetMyTime ( "AvailableTill" ),
                Queue = reader.GetMyUInt ( "Queue" ),
                InsidePerformances =  reader.GetMyUInt ( "InsidePerformance" ),
                OutsidePerformances = reader.GetMyUInt ( "OutsidePerformance" ),
                Confirmed = reader.GetMyDateTime ( "Confirmed" )
            }, parameters );
    }
    #endregion

    #region Stages
    public Task<List<PlanningStagesModel>> GetPlanningStagesAsync(int _festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningStages,
           reader => new PlanningStagesModel
           {
                PodiumId = reader.GetMyUInt( "PodiumId" ),
                Name = reader.GetMyString ( "Name" ),
                PerformanceLocation = reader.GetMyString ( "PerformanceLocation" ),
                Type = reader.GetMyString ( "Type" ),
                Quality = reader.GetMyUInt( "Quality" ),
                MaxSingers = reader.GetMyUInt( "MaxSingers" ),
                Volunteers = reader.GetMyString ( "Volunteers" ),
                Opening = reader.GetMyTime ( "Opening" ),
                Closing = reader.GetMyTime ( "Closing" ),
                VolunteersFrom = reader.GetMyTime ( "VolunteersFrom" ),
                VolunteersTill = reader.GetMyTime ( "VolunteersTill" ),
                MapNumber = reader.GetMyUInt( "MapNumber" )
           }, parameters );
    }
    #endregion

    #region StageTypes
    public Task<List<PlanningStageTypesModel>> GetPlanniningStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningStageTypes,
            reader => new PlanningStageTypesModel
            {
                TypeId = reader.GetMyInt ( "TypeId" ),
                Type = reader.GetMyString ( "Type" ),
                CompatibleWith = reader.GetMyString ( "CompatibleWith" )
            } );
    }
    #endregion

    #region Volunteers
    public Task<List<PlanningVolunteersModel>> GetPlanningVolunteersAsync( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteers,
            reader => new PlanningVolunteersModel
            {
                VolunteerId = reader.GetMyUInt ( "VolunteerId" ),
                Date = reader.GetMyDateTime ( "Date" ),
                FestivalId = reader.GetMyUInt( "FestivalId" ),
                PersonId = reader.GetMyUInt( "PersonId" ),
                PersonName = reader.GetMyString ( "PersonName" ),
                AvailableFrom = reader.GetMyTime ( "AvailableFrom" ),
                AvailableTill = reader.GetMyTime ( "AvailableTill" ),
                ChainedHours = reader.GetMyUInt( "ChainedHours" ),
                Lunch = reader.GetMyString( "Lunch" ),
                Vegetarian = reader.GetMyString( "Vegetarian" ),
                Meeting = reader.GetMyString( "Meeting" ),
                Experience = reader.GetMyString( "Experience" ),
                StageDuty = reader.GetMyString( "StageDuty" ),
                Tasks = reader.GetMyString( "Tasks" ),
                TogetherWithId = reader.GetMyUInt( "TogetherWithId" ),
                TogetherWithName = reader.GetMyString( "TogetherWithName" ),
                PreferedStage =    reader.GetMyUInt ( "PreferedStage" ),
                DisapprovedStage = reader.GetMyUInt ( "DisapprovedStage" ),
                PreferedGroup =    reader.GetMyUInt ( "PreferedGroup" ),
                DisapprovedGroup = reader.GetMyUInt( "DisapprovedGroup" ),
                PreferedTask = reader.GetMyString ( "PreferedTask" ),
                DisapprovedTask = reader [ "DisapprovedTask" ].ToString() ?? string.Empty,
                Notes = reader.GetMyString ( "Notes" )
            }, parameters );
    }
    #endregion

    #region Volunteer Tasks
    public Task<List<PlanningVolunteerTasksModel>> GetPlanningVolunteerTasksAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteerTasks,
            reader => new PlanningVolunteerTasksModel
            {
                TaakId = reader.GetMyUInt( "TaakId" ),
                ShortName = reader.GetMyString ( "ShortName" ),
                Name = reader.GetMyString ( "Name" ),
                MinimumTime = reader.GetMyUInt ( "MinimumTime" ),
                MaximumTime = reader.GetMyUInt( "MaximumTime" ),
                Timeslot1From = reader.GetMyTime ( "Timeslot1From" ),
                Timeslot1Till = reader.GetMyTime ( "Timeslot1Till" ),
                Timeslot1Volunteers = reader.GetMyInt( "Timeslot1Volunteers" ),
                Timeslot2From = reader.GetMyTime ( "Timeslot2From" ),
                Timeslot2Till = reader.GetMyTime ( "Timeslot2Till" ),
                Timeslot2Volunteers = reader.GetMyInt( "Timeslot2Volunteers" )
            } );
    }
    #endregion

    #region Volunteers Task Occupancy
    public async Task<bool> HasPlanningVolunteerTaskOccupancy( int festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        var result = await _dataService.ExecuteQueryAsync(QueryDefinitions.HasPlanningVolunteerTaskOccupancy,
            reader => reader.GetBoolean(reader.GetOrdinal("HasRows")),
        parameters);

        return result.First();
    }

    public Task<List<PlanningVolunteerTaskOccupancyModel>> GetPlanningVolunteerTaskOccupancyAsync( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteerTaskOccupancy,
            reader => new PlanningVolunteerTaskOccupancyModel
            {
                TaskId = reader.GetMyUInt( "TaskId" ),
                TaskName = reader.GetMyString ( "TaskName" ),
                PersonId = reader.GetMyUInt( "PersonId" ),
                PersonName = reader.GetMyString ( "PersonName" ),
                StageId = reader.GetMyInt( "StageId" ),
                StageName = reader.GetMyString ( "StageName" ),
                From = reader.GetMyTime ( "From" ),
                Till = reader.GetMyTime ( "Till" ),
                Pinned = reader.GetMyString ( "Pinned" )
            }, parameters );
    }
    #endregion

    #region Planning
    #region Conditions
    public Task<List<PlanningConditionsModel>> GetPlanningConditionsAsync( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningConditionsOverview,
           reader => new PlanningConditionsModel
           {
               WishTimeBetweenPerformances = reader.GetMyInt( "WishTimeBetweenPerformances" ),
               MaxTimeBetweenPerformances = reader.GetMyInt( "MaxTimeBetweenPerformances" ),
               MaxLentgVolunteersShift = reader.GetMyInt( "MaxLentgVolunteersShift" ),
               PenaltyInteruptionPerformances = reader.GetMyInt( "PenaltyInteruptionPerformances" ),
               TasknamesWithoutSwitchTime = reader.GetMyString( "TasknamesWithoutSwitchTime" ),
               SubstitudeTaskName = reader.GetMyString( "SubstitudeTaskName" ),
               PerformanceTime = reader.GetMyInt( "PerformanceTime" )
           }, parameters );
    }
    #endregion

    #region Stageduty
    public Task<List<PlanningStageVolunteersModel>> GetPlanningVolunteersPerStageOverview( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningVolunteersPerStageOverview,
           reader => new PlanningStageVolunteersModel
           {
               StageNumber = reader.GetMyInt( "StageNumber" ),
               StageId = reader.GetMyInt( "StageId" ),
               StageName = reader.GetMyString( "StageName" ),
               Volunteer = reader.GetMyString( "Volunteer" ),
               StartTime = reader.IsDBNull( "StartTime" ) ? null : reader.GetMyTime( "StartTime" ),
               EndTime = reader.IsDBNull( "EndTime" ) ? null : reader.GetMyTime( "EndTime" )
           }, parameters );
    }
    #endregion

    #region Other duty
    public Task<List<PlanningOtherVolunteerTasksModel>> GetPlanningOtherVolunteerTasksOverview( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningOtherVolunteerTasksOverview,
           reader => new PlanningOtherVolunteerTasksModel
           {
               TaskName = reader.GetMyString( "Task" ),
               Volunteer = reader.GetMyString( "Volunteer" ),
               StartTime = reader.IsDBNull( "StartTime" ) ? null : reader.GetMyTime( "StartTime" ),
               EndTime = reader.IsDBNull( "EndTime" ) ? null : reader.GetMyTime( "EndTime" )
           }, parameters );
    }
    #endregion

    #region Planned Performances
    public Task<List<StagePerformanceModel>> GetStagePerformancesAsync (int _festivalId )
    {
        var parameters = new Dictionary<string, object>
    {
        { "@FestivalId", _festivalId }
    };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningPerformancesOverview,
            reader => new StagePerformanceModel
            {
                SortOrder = reader.GetInt32( "SortOrder" ),
                StageId = reader.GetInt32( "StageId" ),
                StageName = reader.GetMyString( "StageName" ),
                Timeslot = reader.GetInt32( "Timeslot" ),
                GroupName = reader.GetMyString( "GroupName" )
            },
            parameters
        );
    }
    #endregion
    #region XML Export
    #region Export Full Planning To Xml
    // -------------------------------------------------------
    // MAIN: Full XML export in "<channel>/<table>/<row>" vorm
    // -------------------------------------------------------
    public async Task ExportFullPlanningToXmlAsync( int festivalId, string filePath )
    {
        var channel = new XElement("channel");

        // Add tables in the order you want them exported
        channel.Add( await BuildTableElementAsync(
            "ah_podium_type_relaties",
            QueryDefinitions.GetPlanningStageTypeRelations,
            new() { { "@FestivalId", festivalId } },
            ["vervangt_podium_type_id"] // Split this collumn in separate rows

        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_podium_genre_relaties",
            QueryDefinitions.GetPlanningStageGenreRelations,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_podium_koor_relaties",
            QueryDefinitions.GetPlanningStageGroupRelations,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_festivals",
            QueryDefinitions.GetPlanningFestivals,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildKeyValueTableAsync(
            "ah_voorwaarden",
            QueryDefinitions.GetPlanningConditions,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_podium_types",
            QueryDefinitions.GetPlanningStageTypes,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_genres",
            QueryDefinitions.GetPlanningGenres,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_inschrijvingen",
            QueryDefinitions.GetPlanningRegistrations,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_personen_rollen",
            QueryDefinitions.GetPlanningPersonRoles,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_personen",
            QueryDefinitions.GetPlanningPersons,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_podia",
            QueryDefinitions.GetPlanningStages,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_zanggroepen",
            QueryDefinitions.GetPlanningGroups,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
                    "ah_vrijwilligers",
                    QueryDefinitions.GetPlanningVolunteers,
                    new() { { "@FestivalId", festivalId } }
                ) );

        channel.Add( await BuildTableElementAsync(
            "ah_optredens",
            QueryDefinitions.GetPlanningPerformances,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_vrijwilligersdiensten",
            QueryDefinitions.GetPlanningVolunteerShifts,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_taken",
            QueryDefinitions.GetPlanningVolunteerTasks,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync(
            "ah_taken",
            QueryDefinitions.GetPlanningVolunteerTaskOccupancy,
            new() { { "@FestivalId", festivalId } }
        ) );

        var doc = new XDocument(channel);

        Directory.CreateDirectory( Path.GetDirectoryName( filePath )! );

        await using var writer = File.CreateText(filePath);
        doc.Save( writer );
    }
    #endregion

    #region Export Full Planning To Excel
    public async Task ExportFullPlanningToExcelAsync( int festivalId, string filePath )
    {
        // Build the same channel structure as your XML export
        var channel = new XElement("channel");

        channel.Add( await BuildTableElementAsync( "ah_podium_type_relaties",
            QueryDefinitions.GetPlanningStageTypeRelations,
            new() { { "@FestivalId", festivalId } },
            [ "vervangt_podium_type_id" ]
        ) );

        channel.Add( await BuildTableElementAsync( "ah_podium_genre_relaties",
            QueryDefinitions.GetPlanningStageGenreRelations,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_podium_koor_relaties",
            QueryDefinitions.GetPlanningStageGroupRelations,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_festivals",
            QueryDefinitions.GetPlanningFestivals,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildKeyValueTableAsync( "ah_voorwaarden",
            QueryDefinitions.GetPlanningConditions,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_podium_types",
            QueryDefinitions.GetPlanningStageTypes,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_genres",
            QueryDefinitions.GetPlanningGenres,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_inschrijvingen",
            QueryDefinitions.GetPlanningRegistrations,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_personen_rollen",
            QueryDefinitions.GetPlanningPersonRoles,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_personen",
            QueryDefinitions.GetPlanningPersons,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_podia",
            QueryDefinitions.GetPlanningStages,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_zanggroepen",
            QueryDefinitions.GetPlanningGroups,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_vrijwilligers",
            QueryDefinitions.GetPlanningVolunteers,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_optredens",
            QueryDefinitions.GetPlanningPerformances,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_vrijwilligersdiensten",
            QueryDefinitions.GetPlanningVolunteerShifts,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_taken",
            QueryDefinitions.GetPlanningVolunteerTasks,
            new() { { "@FestivalId", festivalId } }
        ) );

        channel.Add( await BuildTableElementAsync( "ah_taken_bezetting",
            QueryDefinitions.GetPlanningVolunteerTaskOccupancy,
            new() { { "@FestivalId", festivalId } }
        ) );

        // --- Build Excel ---
        using var workbook = new XLWorkbook();

        foreach ( var table in channel.Elements( "table" ) )
        {
            string tableName = table.Attribute("name")?.Value ?? "Unknown";

            var ws = workbook.Worksheets.Add(tableName);

            // Extract all rows
            var rows = table.Elements("row").ToList();
            if ( !rows.Any() )
                continue; // Empty tab

            // Determine all column names from the first row
            var firstRow = rows.First();
            var columns = firstRow.Elements().Select(e => e.Name.LocalName).ToList();

            // Write header
            for ( int c = 0; c < columns.Count; c++ )
            {
                ws.Cell( 1, c + 1 ).Value = columns [ c ];
                ws.Cell( 1, c + 1 ).Style.Font.Bold = true;
            }

            // Write data rows
            int r = 2;
            foreach ( var row in rows )
            {
                for ( int c = 0; c < columns.Count; c++ )
                {
                    var colName = columns[c];
                    var el = row.Element(colName);
                    ws.Cell( r, c + 1 ).Value = el?.Value ?? "";
                }
                r++;
            }

            // Make it a table (with filters & sorting enabled)
            var rng = ws.Range(1, 1, r - 1, columns.Count);
            rng.CreateTable();

            ws.Columns().AdjustToContents();
        }

        Directory.CreateDirectory( Path.GetDirectoryName( filePath )! );
        workbook.SaveAs( filePath );
    }
    private async Task ExportToExcelAsync()
    {
        //    // LEGE placeholder – vullen we zodra de XML werkt
        await Task.CompletedTask;
    }
    #endregion

    #region Build Table Element
    // -------------------------------------------------------
    // Builds a <table>...</table> including its <row> items
    // -------------------------------------------------------
    private async Task<XElement> BuildTableElementAsync(
    string tableName,
    string sql,
    Dictionary<string, object>? parameters = null,
    IEnumerable<string>? splitColumns = null )
    {
        var tableEl = new XElement("table",
        new XAttribute("name", tableName));

        // If SQL is null or empty → return empty table element
        if ( string.IsNullOrWhiteSpace( sql ) )
            return tableEl;

        await _dataService.ExecuteReaderAsync( sql, async reader =>
        {
            while ( await reader.ReadAsync() )
            {
                var rowEl = new XElement("row");

                for ( int i = 0; i < reader.FieldCount; i++ )
                {
                    string field = reader.GetName(i);
                    object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    // Check if this column should be split
                    if ( value != null && splitColumns != null && splitColumns.Contains( field ) )
                    {
                        var tokens = value.ToString()!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        foreach ( var token in tokens )
                        {
                            var col = new XElement(field, token);
                            rowEl.Add( col );
                        }
                    }
                    else
                    {
                        var col = new XElement(field);
                        if ( value != null )
                            col.Value = ConvertToString( value );
                        rowEl.Add( col );
                    }
                }

                tableEl.Add( rowEl );
            }
        }, parameters );

        return tableEl;
    }

    // -------------------------------------------------------
    // Builds a <table>...</table> including its <row> items
    // When The table column and value shoul appear in seperate rows
    // -------------------------------------------------------
    private async Task<XElement> BuildKeyValueTableAsync(
    string tableName,
    string sql,
    Dictionary<string, object> parameters )
    {
        var tableEl = new XElement("table", new XAttribute("name", tableName));

        await _dataService.ExecuteReaderAsync( sql, async reader =>
        {
            if ( await reader.ReadAsync() )
            {
                for ( int i = 0; i < reader.FieldCount; i++ )
                {
                    var colName = reader.GetName(i);
                    var colValue = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();

                    var rowEl = new XElement("row",
                    new XElement("naam", colName),
                    new XElement("waarde", colValue)
                );

                    tableEl.Add( rowEl );
                }
            }
        }, parameters );

        return tableEl;
    }
    #endregion

    #region Converts database types to XML-safe string values
    // -------------------------------------------------------
    // Converts database types to XML-safe string values
    // -------------------------------------------------------
    public static string? ConvertToString( object value )
    {
        // Dates → ISO format
        if ( value is DateTime dt )
            return dt.ToString( "yyyy-MM-dd" );

        if ( value is DateOnly d )
            return d.ToString( "yyyy-MM-dd" );

        if ( value is TimeSpan ts )
            return ts.ToString( "c" );

        return value?.ToString();
    }
    #endregion
    #endregion
    #endregion
}