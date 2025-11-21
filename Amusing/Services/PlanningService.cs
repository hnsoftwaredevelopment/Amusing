using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class PlanningService ( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    #region Conditions
    public Task<List<PlanningConditionsModel>> GetPlanningConditionsAsync( int _festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningConditions,
           reader => new PlanningConditionsModel
           {
                WishTimeBetweenPerformances = Convert.ToInt32( reader [ "WìshTimeBetweenPerformances" ] ),
                MaxTimeBetweenPerformances = Convert.ToInt32( reader [ "MaxTimeBetweenPerformances" ] ),
                MaxLentgVolunteersShift = Convert.ToInt32( reader [ "MaxLentgVolunteersShift" ] ),
                PenaltyInteruptionPerformances = Convert.ToInt32( reader [ "PenaltyInteruptionPerformances" ] ),
                TasknamesWithoutSwitchTime = reader [ "TasknamesWithoutSwitchTime" ].ToString() ?? "Vrijwilligersbalie;Garderobe",
                SubstitudeTaskName = reader [ "SubstitudeTaskName" ].ToString() ?? "Reserve voor oproep"
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }
    #endregion

    #region Festivals
    public Task<List<PlanningFestivalsModel>> GetPlanningFestivalsAsync( int _festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningFestivals,
           reader => new PlanningFestivalsModel
           {
               FestivalId = Convert.ToUInt32( reader [ "FestivalId" ] ),
               Festival = $"Amusing Hengelo {reader [ "Festival" ]}",
               PerformanceLength = 30,
               StartFestivalday = TimeOnly.Parse( reader [ "StartFestivalday" ].ToString() ?? "00:00" ),
               EndFestivalday = TimeOnly.Parse( reader [ "EndFestivalday" ].ToString() ?? "00:00" ),
               StartPause = TimeOnly.Parse( reader [ "StartPause" ].ToString() ?? "00:00" ),
               EndPause = TimeOnly.Parse( reader [ "EndPause" ].ToString() ?? "00:00" ),
               EndExperiencedSubstitude = TimeOnly.Parse( reader [ "EndExperiencedSubstitude" ].ToString() ?? "00:00" )
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }
    #endregion

    #region Genres
    public Task<List<PlanningGenresModel>> GetPlanningGenresAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningGenres,
            reader => new PlanningGenresModel
            {
                GenreId = Convert.ToInt32( reader [ "GenreId" ] ),
                Name = reader [ "Name" ].ToString()
            } );
    }
    #endregion

    #region Groups
    public Task<List<PlanningGroupsModel>> GetPlanningGroupsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningGroups,
            reader => new PlanningGroupsModel
            {
                GroupId = Convert.ToUInt32( reader [ "GroupId" ] ),
                Name = reader [ "Name" ].ToString(),
                GenreId = Convert.ToUInt32( reader [ "GenreId" ] ),
                City = reader [ "City" ].ToString(),
                Country = reader [ "Country" ].ToString()
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

        return result.First();
    }

    public Task<List<PlanningPerformancesModel>> GetPlanningPerformancesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningPerformances,
        reader =>
        {
            var stageName = reader["StageName"].ToString() ?? string.Empty;
            var fromTime = TimeOnly.Parse(reader["From"].ToString() ?? "00:00");
            var groupName = reader["GroupName"].ToString() ?? string.Empty;

            return new PlanningPerformancesModel
            {
                FestivalId = Convert.ToUInt32( reader [ "FestivalId" ] ),
                GroupId = Convert.ToUInt32( reader [ "GroupId" ] ),
                GroupName = groupName,
                TimeSlotId = Convert.ToUInt32( reader [ "TimeSlotId" ] ),
                StageId = Convert.ToUInt32( reader [ "StageId" ] ),
                StageName = stageName,
                From = fromTime,
                To = TimeOnly.Parse( reader [ "To" ].ToString() ?? "00:00" ),
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
                PersonId = Convert.ToUInt32( reader [ "PersonId" ] ),
                PersonName = reader [ "PersonName" ].ToString() ?? string.Empty,
                GroupId = Convert.ToInt32( reader [ "GroupId" ] ),
                GroupName = reader [ "GroupName" ].ToString() ?? string.Empty,
                Role = reader [ "Role" ].ToString() ?? string.Empty
            } );
    }
    #endregion

    #region Persons
    public Task<List<PlanningPersonsModel>> GetPlanningPersonsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningPersons,
            reader => new PlanningPersonsModel
            {
                PersonId = Convert.ToUInt32( reader [ "PersonId" ] ),
                FirstName = reader [ "FirstName" ].ToString() ?? string.Empty,
                Affix = reader [ "Affix" ].ToString() ?? string.Empty,
                Surname = reader [ "Surname" ].ToString() ?? string.Empty,
                Name = reader [ "Name" ].ToString() ?? string.Empty
            } );
    }
    #endregion

    #region Registrations
    public Task<List<PlanningRegistrationsModel>> GetPlanningRegistrationsAsync(int _festivalId)
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningRegistrations,
            reader => new PlanningRegistrationsModel
            {
                FestivalId = Convert.ToUInt32( reader [ "FestivalId" ] ),
                GroupId = Convert.ToUInt32( reader [ "GroupId" ] ),
                GroupName = reader [ "GroupName" ].ToString() ?? string.Empty,
                Wish1 = reader [ "Wish1" ].ToString() ?? string.Empty,
                Wish2 = reader [ "Wish2" ].ToString() ?? string.Empty,
                Wish3 = reader [ "Wish3" ].ToString() ?? string.Empty,
                Wish4 = reader [ "Wish4" ].ToString() ?? string.Empty,
                Singers = Convert.ToUInt32( reader [ "Singers" ] ),
                Stagetype = reader [ "Stagetype" ].ToString() ?? string.Empty,
                ForcedStageChoice = Convert.ToInt32( reader [ "ForcedStageChoice" ] ),
                Registered = Convert.ToDateTime( reader [ "Registered" ] ),
                AvailableFrom = TimeOnly.Parse( reader [ "AvailableFrom" ].ToString() ?? "00:00" ),
                AvailableTill = TimeOnly.Parse( reader [ "AvailableTill" ].ToString() ?? "00:00" ),
                Queue = Convert.ToUInt32( reader [ "Queue" ] ),
                InsidePerformances = Convert.ToUInt32( reader [ "InsidePerformance" ] ),
                OutsidePerformances = Convert.ToUInt32( reader [ "OutsidePerformance" ] ),
                Confirmed = Convert.ToDateTime( reader [ "Confirmed" ] )
            },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } } );
    }
    #endregion

    #region Stages
    public Task<List<PlanningStagesModel>> GetPlanningStagesAsync(int _festivalId)
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningStages,
           reader => new PlanningStagesModel
           {
                PodiumId = Convert.ToUInt32( reader [ "PodiumId" ] ),
                Name = reader [ "Name" ].ToString() ?? string.Empty,
                PerformanceLocation = reader [ "PerformanceLocation" ].ToString() ?? string.Empty,
                Type = reader [ "Type" ].ToString() ?? string.Empty,
                Quality = Convert.ToUInt32( reader [ "Quality" ] ),
                MaxSingers = Convert.ToUInt32( reader [ "MaxSingers" ] ),
                Volunteers = reader [ "Volunteers" ].ToString() ?? string.Empty,
                Opening = TimeOnly.Parse( reader [ "Opening" ].ToString() ?? "00:00" ),
                Closing = TimeOnly.Parse( reader [ "Closing" ].ToString() ?? "00:00" ),
                VolunteersFrom = TimeOnly.Parse( reader [ "VolunteersFrom" ].ToString() ?? "00:00" ),
                VolunteersTill = TimeOnly.Parse( reader [ "VolunteersTill" ].ToString() ?? "00:00" ),
                MapNumber = Convert.ToUInt32( reader [ "MapNumber" ] )
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }
    #endregion

    #region StageTypes
    public Task<List<PlanningStageTypesModel>> GetPlanniningStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningStageTypes,
            reader => new PlanningStageTypesModel
            {
                TypeId = Convert.ToInt32( reader [ "TypeId" ] ),
                Type = reader [ "Type" ].ToString(),
                CompatibleWith = reader [ "CompatibleWith" ].ToString()
            } );
    }
    #endregion

    #region Volunteers
    public Task<List<PlanningVolunteersModel>> GetPlanningVolunteersAsync( int _festivalId )
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteers,
            reader => new PlanningVolunteersModel
            {
                VolunteerId = Convert.ToUInt32( reader [ "VolunteerId" ] ),
                Date = Convert.ToDateTime( reader [ "Date" ] ),
                FestivalId = Convert.ToUInt32( reader [ "FestivalId" ] ),
                PersonId = Convert.ToUInt32( reader [ "PersonId" ] ),
                PersonName = reader [ "PersonName" ].ToString() ?? string.Empty,
                AvailableFrom = TimeOnly.Parse( reader [ "AvailableFrom" ].ToString() ?? "00:00" ),
                AvailableTill = TimeOnly.Parse( reader [ "AvailableTill" ].ToString() ?? "00:00" ),
                ChainedHours = Convert.ToUInt32( reader [ "ChainedHours" ] ),
                Lunch = reader [ "Lunch" ].ToString() ?? string.Empty,
                Vegetarian = reader [ "Vegetarian" ].ToString() ?? string.Empty,
                Meeting = reader [ "Meeting" ].ToString() ?? string.Empty,
                Experience = reader [ "Experience" ].ToString() ?? string.Empty,
                StageDuty = reader [ "StageDuty" ].ToString() ?? string.Empty,
                Tasks =  reader [ "Tasks" ].ToString() ?? string.Empty,
                TogetherWithId = Convert.ToUInt32( reader [ "TogetherWithId" ] ),
                TogetherWithName = reader [ "TogetherWithName" ].ToString() ?? string.Empty,
                PreferedStage = Convert.ToUInt32( reader [ "PreferedStage" ] ),
                DisapprovedStage = Convert.ToUInt32( reader [ "DisapprovedStage" ] ),
                PreferedGroup = Convert.ToUInt32( reader [ "PreferedGroup" ] ),
                DisapprovedGroup = Convert.ToUInt32( reader [ "DisapprovedGroup" ] ),
                PreferedTask = reader [ "PreferedTask" ].ToString() ?? string.Empty,
                DisapprovedTask = reader [ "DisapprovedTask" ].ToString() ?? string.Empty,
                Notes = reader [ "Notes" ].ToString() ?? string.Empty
            },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } } );
    }
    #endregion

    #region Volunteer Tasks
    public Task<List<PlanningVolunteerTasksModel>> GetPlanningVolunteerTasksAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteerTasks,
            reader => new PlanningVolunteerTasksModel
            {
                TaakId = Convert.ToUInt32( reader [ "TaakId" ] ),
                ShortName = reader [ "ShortName" ].ToString() ?? string.Empty,
                Name = reader [ "Name" ].ToString() ?? string.Empty,
                MinimumTime = Convert.ToUInt32( reader [ "MinimumTime" ] ),
                MaximumTime = Convert.ToUInt32( reader [ "MaximumTime" ] ),
                Timeslot1From = TimeOnly.Parse( reader [ "Timeslot1From" ].ToString() ?? "00:00" ),
                Timeslot1Till = TimeOnly.Parse( reader [ "Timeslot1Till" ].ToString() ?? "00:00" ),
                Timeslot1Volunteers = Convert.ToInt32( reader [ "Timeslot1Volunteers" ] ),
                Timeslot2From = TimeOnly.Parse( reader [ "Timeslot2From" ].ToString() ?? "00:00" ),
                Timeslot2Till = TimeOnly.Parse( reader [ "Timeslot2Till" ].ToString() ?? "00:00" ),
                Timeslot2Volunteers = Convert.ToInt32( reader [ "Timeslot2Volunteers" ] )
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
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPlanningVolunteerTaskOccupancy,
            reader => new PlanningVolunteerTaskOccupancyModel
            {
                TaskId = Convert.ToUInt32( reader [ "TaskId" ] ),
                TaskName = reader [ "TaskName" ].ToString() ?? string.Empty,
                PersonId = Convert.ToUInt32( reader [ "PersonId" ] ),
                PersonName = reader [ "PersonName" ].ToString() ?? string.Empty,
                StageId = Convert.ToInt32( reader [ "StageId" ] ),
                StageName = reader [ "StageName" ].ToString() ?? string.Empty,
                From = TimeOnly.Parse( reader [ "From" ].ToString() ?? "00:00" ),
                Till = TimeOnly.Parse( reader [ "Till" ].ToString() ?? "00:00" ),
                Pinned = reader [ "Pinned" ].ToString() ?? string.Empty
            },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } } );
    }
    #endregion
}
