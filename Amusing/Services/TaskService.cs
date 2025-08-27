using System.Text.Json;

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
                TimeBlock1Volunteers = Convert.ToInt16( reader [ "TimeBlock1Volunteers" ] ),
                TimeBlock2From = reader [ "TimeBlock2From" ] == DBNull.Value
                    ? null
                    : TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "TimeBlock2From" ] ),
                TimeBlock2Until = reader [ "TimeBlock2Until" ] == DBNull.Value
                    ? null
                    : TimeOnly.FromTimeSpan( ( TimeSpan ) reader [ "TimeBlock2Until" ] ),
                TimeBlock2Volunteers = Convert.ToInt16( reader [ "TimeBlock2Volunteers" ] ),
                Description = reader [ "Description" ].ToString(),
                Active = reader [ "Active" ].ToString().ToLower()
            } );
    }
    public async Task UpdateTaskAsync( TaskModel model )
    {
        string _occupation = GetTimeBlock(model);

        Dictionary<string, object> parameters = new()
        {
            { "@TaskId", model.TaskId },
            { "@ShortName", model.ShortName },
            { "@Name", model.Name },
            { "@MinTimeSpan", model.MinTimeSpan },
            { "@MaxTimeSpan", model.MaxTimeSpan },
            { "@Occupation", _occupation },
            { "@TimeBlock1From", model.TimeBlock1From },
            { "@TimeBlock1Until", model.TimeBlock1Until },
            { "@TimeBlock1Volunteers", model.TimeBlock1Volunteers },
            { "@TimeBlock2From", model.TimeBlock2From },
            { "@TimeBlock2Until", model.TimeBlock2Until },
            { "@TimeBlock2Volunteers", model.TimeBlock2Volunteers },
            { "@Active", model.Active },
            { "@Description", model.Description }
         };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyTaskByTaskId, parameters );
    }
    public async Task TaskActivationAsync( TaskModel model )
    {
        // Switch the activation status
        string _active = "ja";
        if ( model.ActiveBool )
        { _active = "nee"; }

        Dictionary<string, object> parameters = new()
        {
            { "@TaskId", model.TaskId },
            { "@Active", _active }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.TaskActivationByTaskId, parameters );
    }
    public async Task<uint> AddTaskAsync( TaskModel model )
    {
        string _occupation = GetTimeBlock(model);

        Dictionary<string, object> parameters = new()
        {
            { "@ShortName", model.ShortName },
            { "@Name", model.Name },
            { "@MinTimeSpan", model.MinTimeSpan },
            { "@MaxTimeSpan", model.MaxTimeSpan },
            { "@Occupation", _occupation },
            { "@TimeBlock1From", model.TimeBlock1From },
            { "@TimeBlock1Until", model.TimeBlock1Until },
            { "@TimeBlock1Volunteers", model.TimeBlock1Volunteers },
            { "@TimeBlock2From", model.TimeBlock2From },
            { "@TimeBlock2Until", model.TimeBlock2Until },
            { "@TimeBlock2Volunteers", model.TimeBlock2Volunteers },
            { "@Active", model.Active },
            { "@Description", model.Description }
        };

        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewTask, parameters );
    }
    public string GetTimeBlock( TaskModel model )
    {
        List<TimeBlock> timeBlocks = new();

        if ( !string.IsNullOrWhiteSpace( model.TimeBlock1From.ToString() ) &&
            !string.IsNullOrWhiteSpace( model.TimeBlock1Until.ToString() ) )
        {
            timeBlocks.Add( new TimeBlock
            {
                From = model.TimeBlock1From.ToString(),
                Until = model.TimeBlock1Until.ToString(),
                Number = model.TimeBlock1Volunteers.ToString() ?? "0"
            } );
        }

        if ( !string.IsNullOrWhiteSpace( model.TimeBlock2From.ToString() ) &&
            !string.IsNullOrWhiteSpace( model.TimeBlock2Until.ToString() ) )
        {
            timeBlocks.Add( new TimeBlock
            {
                From = model.TimeBlock2From.ToString(),
                Until = model.TimeBlock2Until.ToString(),
                Number = model.TimeBlock2Volunteers.ToString() ?? "0"
            } );
        }

        return JsonSerializer.Serialize( timeBlocks );
    }

    public class TimeBlock
    {
        public string From { get; set; }
        public string Until { get; set; }
        public string Number { get; set; }
    }
}