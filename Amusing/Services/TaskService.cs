using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class TaskService(GenericDataService dataService)
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<TaskModel>> GetActiveTasksAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetActiveTasks,
            reader => new TaskModel
            {
                TaakId = Convert.ToUInt32(reader["taakid"]),
                Naam = reader["naam"].ToString() ?? string.Empty,
                MinimumDuur = reader["minimumduur"].ToString() ?? string.Empty,
                MaximumDuur = reader["maximumduur"].ToString() ?? string.Empty,
                Van = reader["van"].ToString() ?? string.Empty,
                Tot = reader["tot"].ToString() ?? string.Empty,
                Aantal = reader["aantal"].ToString() ?? string.Empty
            });
    }
    public Task<List<TaskModel>> GetInActiveTasksAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetInActiveTasks,
            reader => new TaskModel
            {
                TaakId = Convert.ToUInt32(reader["taakid"]),
                Naam = reader["naam"].ToString() ?? string.Empty,
                MinimumDuur = reader["minimumduur"].ToString() ?? string.Empty,
                MaximumDuur = reader["maximumduur"].ToString() ?? string.Empty,
                Van = reader["van"].ToString() ?? string.Empty,
                Tot = reader["tot"].ToString() ?? string.Empty,
                Aantal = reader["aantal"].ToString() ?? string.Empty,
            });
    }
}