using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class TaskService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<TaskModel>> GetActiveTasksAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetActiveTasks,
            reader => new TaskModel
            {
                TaakId = Convert.ToUInt32( reader [ "taakid" ] ),
                Naam = reader [ "naam" ].ToString() ?? string.Empty,
                MinimumDuur = reader [ "minimumduur" ].ToString() ?? string.Empty,
                MaximumDuur = reader [ "maximumduur" ].ToString() ?? string.Empty,
                Van = reader [ "van" ].ToString() ?? string.Empty,
                Tot = reader [ "tot" ].ToString() ?? string.Empty,
                Aantal = reader [ "aantal" ].ToString() ?? string.Empty
            } );
    }
    public Task<List<TaskModel>> GetInActiveTasksAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetInActiveTasks,
            reader => new TaskModel
            {
                TaakId = Convert.ToUInt32( reader [ "taakid" ] ),
                Naam = reader [ "naam" ].ToString() ?? string.Empty,
                MinimumDuur = reader [ "minimumduur" ].ToString() ?? string.Empty,
                MaximumDuur = reader [ "maximumduur" ].ToString() ?? string.Empty,
                Van = reader [ "van" ].ToString() ?? string.Empty,
                Tot = reader [ "tot" ].ToString() ?? string.Empty,
                Aantal = reader [ "aantal" ].ToString() ?? string.Empty,
            } );
    }
    public Task<List<TaskModel>> GetAllTasksAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetAllTasks,
            reader => new TaskModel
            {
                TaskId = Convert.ToUInt32( reader [ "TaskId" ] ),
                ShortName = reader [ "ShortName" ].ToString() ?? string.Empty,
                Name = reader [ "Name" ].ToString() ?? string.Empty,
                MinTimeSpan = Convert.ToInt16( reader [ "MinTimeSpan" ] ),
                MaxTimeSpan = Convert.ToInt16( reader [ "MaxTimeSpan" ] ),
                TimeBlock1From = reader [ "TimeBlock1From" ] == DBNull.Value
                    ? null
                    : TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "TimeBlock1From" ] ),
                TimeBlock1Until = reader [ "TimeBlock1Until" ] == DBNull.Value
                    ? null
                    : TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "TimeBlock1Until" ] ),
                TimeBlock1Volunteers = Convert.ToUInt16( reader [ "TimeBlock1Volunteers" ] ),
                TimeBlock2From = reader [ "TimeBlock2From" ] == DBNull.Value
                    ? null
                    : TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "TimeBlock2From" ] ),
                TimeBlock2Until = reader [ "TimeBlock2Until" ] == DBNull.Value
                    ? null
                    : TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "TimeBlock2Until" ] ),
                TimeBlock2Volunteers = Convert.ToUInt16( reader [ "TimeBlock2Volunteers" ] ),
                Description = reader [ "Description" ].ToString(),
                Active = reader [ "Active" ].ToString().ToLower()
            } );
    }
}