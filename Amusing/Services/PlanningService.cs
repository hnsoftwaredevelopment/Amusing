using System.Xml.Linq;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services.Extensions;

namespace Amusing.Services;

public class PlanningService ( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    #region Conditions
    public Task<List<PlanningConditionsModel>> GetPlanningConditionsAsync( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningConditions,
           reader => new PlanningConditionsModel
           {
                WishTimeBetweenPerformances = reader.GetInt( "WishTimeBetweenPerformances" ),
                MaxTimeBetweenPerformances = reader.GetInt( "MaxTimeBetweenPerformances" ),
                MaxLentgVolunteersShift = reader.GetInt( "MaxLentgVolunteersShift" ),
                PenaltyInteruptionPerformances = reader.GetInt( "PenaltyInteruptionPerformances" ),
                TasknamesWithoutSwitchTime = reader.GetString ("TasknamesWithoutSwitchTime" ) ?? "Vrijwilligersbalie;Garderobe",
                SubstitudeTaskName = reader.GetString( "SubstitudeTaskName" ) ?? "Reserve voor oproep"
           }, parameters );
    }
    #endregion

    #region Festivals
    public Task<List<PlanningFestivalsModel>> GetPlanningFestivalsAsync( int _festivalId )
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", _festivalId } } ;

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningFestivals,
           reader => new PlanningFestivalsModel
           {
               FestivalId = reader.GetUInt( "FestivalId" ),
               Festival = $"Amusing Hengelo {reader.GetString ( "Festival" )}",
               PerformanceLength = 30,
               StartFestivalday = reader.GetTime ( "StartFestivalday" ) ,
               EndFestivalday = reader.GetTime ( "EndFestivalday" ),
               StartPause = reader.GetTime ("StartPause" ),
               EndPause = reader.GetTime ( "EndPause" ),
               EndExperiencedSubstitude = reader.GetTime ( "EndExperiencedSubstitude" )
           }, parameters );
    }
    #endregion

    #region Genres
    public Task<List<PlanningGenresModel>> GetPlanningGenresAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningGenres,
            reader => new PlanningGenresModel
            {
                GenreId = reader.GetInt ( "GenreId" ),
                Name = reader.GetString ("Name" )
            } );
    }
    #endregion

    #region Groups
    public Task<List<PlanningGroupsModel>> GetPlanningGroupsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningGroups,
            reader => new PlanningGroupsModel
            {
                GroupId = reader.GetUInt ( "GroupId" ),
                Name = reader.GetString ( "Name" ),
                GenreId = reader.GetUInt ( "GenreId" ),
                City = reader.GetString ( "City" ),
                Country = reader.GetString ( "Country" )
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
            var stageName = reader.GetString ("StageName");
            var fromTime = reader.GetTime ("From");
            var groupName = reader.GetString ( "GroupName");

            return new PlanningPerformancesModel
            {
                FestivalId = reader.GetUInt ( "FestivalId" ),
                GroupId = reader.GetUInt( "GroupId" ),
                GroupName = groupName,
                TimeSlotId = reader.GetUInt( "TimeSlotId" ),
                StageId = reader.GetUInt( "StageId" ),
                StageName = stageName,
                From = fromTime,
                To = reader.GetTime ( "To" ),
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
                PersonId = reader.GetUInt ( "PersonId" ),
                PersonName = reader.GetString ( "PersonName" ),
                GroupId = reader.GetInt ( "GroupId" ),
                GroupName = reader.GetString ( "GroupName" ),
                Role = reader.GetString ( "Role" )
            } );
    }
    #endregion

    #region Persons
    public Task<List<PlanningPersonsModel>> GetPlanningPersonsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningPersons,
            reader => new PlanningPersonsModel
            {
                PersonId = reader.GetUInt ( "PersonId" ),
                FirstName = reader.GetString ( "FirstName" ),
                Affix = reader.GetString ( "Affix" ),
                Surname = reader.GetString ( "Surname" ),
                Name = reader.GetString ( "Name" )
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
                FestivalId = reader.GetUInt ( "FestivalId" ),
                GroupId = reader.GetUInt( "GroupId" ),
                GroupName = reader.GetString ( "GroupName" ),
                Wish1 = reader.GetString ( "Wish1" ),
                Wish2 = reader.GetString ( "Wish2" ),
                Wish3 = reader.GetString ( "Wish3" ),
                Wish4 = reader.GetString ( "Wish4" ),
                Singers = reader.GetUInt ( "Singers" ),
                Stagetype = reader.GetString ( "Stagetype" ),
                ForcedStageChoice = reader.GetInt ( "ForcedStageChoice" ),
                Registered = reader.GetDateTime ( "Registered" ),
                AvailableFrom = reader.GetTime ( "AvailableFrom" ),
                AvailableTill = reader.GetTime ( "AvailableTill" ),
                Queue = reader.GetUInt ( "Queue" ),
                InsidePerformances =  reader.GetUInt ( "InsidePerformance" ),
                OutsidePerformances = reader.GetUInt ( "OutsidePerformance" ),
                Confirmed = reader.GetDateTime ( "Confirmed" )
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
                PodiumId = reader.GetUInt( "PodiumId" ),
                Name = reader.GetString ( "Name" ),
                PerformanceLocation = reader.GetString ( "PerformanceLocation" ),
                Type = reader.GetString ( "Type" ),
                Quality = reader.GetUInt( "Quality" ),
                MaxSingers = reader.GetUInt( "MaxSingers" ),
                Volunteers = reader.GetString ( "Volunteers" ),
                Opening = reader.GetTime ( "Opening" ),
                Closing = reader.GetTime ( "Closing" ),
                VolunteersFrom = reader.GetTime ( "VolunteersFrom" ),
                VolunteersTill = reader.GetTime ( "VolunteersTill" ),
                MapNumber = reader.GetUInt( "MapNumber" )
           }, parameters );
    }
    #endregion

    #region StageTypes
    public Task<List<PlanningStageTypesModel>> GetPlanniningStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningStageTypes,
            reader => new PlanningStageTypesModel
            {
                TypeId = reader.GetInt ( "TypeId" ),
                Type = reader.GetString ( "Type" ),
                CompatibleWith = reader.GetString ( "CompatibleWith" )
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
                VolunteerId = reader.GetUInt ( "VolunteerId" ),
                Date = reader.GetDateTime ( "Date" ),
                FestivalId = reader.GetUInt( "FestivalId" ),
                PersonId = reader.GetUInt( "PersonId" ),
                PersonName = reader.GetString ( "PersonName" ),
                AvailableFrom = reader.GetTime ( "AvailableFrom" ),
                AvailableTill = reader.GetTime ( "AvailableTill" ),
                ChainedHours = reader.GetUInt( "ChainedHours" ),
                Lunch = reader.GetString( "Lunch" ),
                Vegetarian = reader.GetString( "Vegetarian" ),
                Meeting = reader.GetString( "Meeting" ),
                Experience = reader.GetString( "Experience" ),
                StageDuty = reader.GetString( "StageDuty" ),
                Tasks = reader.GetString( "Tasks" ),
                TogetherWithId = reader.GetUInt( "TogetherWithId" ),
                TogetherWithName = reader.GetString( "TogetherWithName" ),
                PreferedStage =    reader.GetUInt ( "PreferedStage" ),
                DisapprovedStage = reader.GetUInt ( "DisapprovedStage" ),
                PreferedGroup =    reader.GetUInt ( "PreferedGroup" ),
                DisapprovedGroup = reader.GetUInt( "DisapprovedGroup" ),
                PreferedTask = reader.GetString ( "PreferedTask" ),
                DisapprovedTask = reader [ "DisapprovedTask" ].ToString() ?? string.Empty,
                Notes = reader.GetString ( "Notes" )
            }, parameters );
    }
    #endregion

    #region Volunteer Tasks
    public Task<List<PlanningVolunteerTasksModel>> GetPlanningVolunteerTasksAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteerTasks,
            reader => new PlanningVolunteerTasksModel
            {
                TaakId = reader.GetUInt( "TaakId" ),
                ShortName = reader.GetString ( "ShortName" ),
                Name = reader.GetString ( "Name" ),
                MinimumTime = reader.GetUInt ( "MinimumTime" ),
                MaximumTime = reader.GetUInt( "MaximumTime" ),
                Timeslot1From = reader.GetTime ( "Timeslot1From" ),
                Timeslot1Till = reader.GetTime ( "Timeslot1Till" ),
                Timeslot1Volunteers = reader.GetInt( "Timeslot1Volunteers" ),
                Timeslot2From = reader.GetTime ( "Timeslot2From" ),
                Timeslot2Till = reader.GetTime ( "Timeslot2Till" ),
                Timeslot2Volunteers = reader.GetInt( "Timeslot2Volunteers" )
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
                TaskId = reader.GetUInt( "TaskId" ),
                TaskName = reader.GetString ( "TaskName" ),
                PersonId = reader.GetUInt( "PersonId" ),
                PersonName = reader.GetString ( "PersonName" ),
                StageId = reader.GetInt( "StageId" ),
                StageName = reader.GetString ( "StageName" ),
                From = reader.GetTime ( "From" ),
                Till = reader.GetTime ( "Till" ),
                Pinned = reader.GetString ( "Pinned" )
            }, parameters );
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
}