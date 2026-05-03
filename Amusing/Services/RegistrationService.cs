using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class RegistrationService(GenericDataService dataService)
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<RegistrationModel>> GetRegistrationsByFestivalIdAsync(uint festivalId)
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetRegistrationsByFestivalId,
            reader => new RegistrationModel
            {
                FestivalId = Convert.ToUInt32(reader["festival_id"]),
                GroepId = Convert.ToUInt32(reader["zanggroep_id"]),
                Datum = Convert.ToDateTime(reader["Datum"]),
                Naam = reader["Naam"].ToString(),
                Stad = reader["Stad"].ToString(),
                Podium = reader["Podium"].ToString(),
                Zangers = Convert.ToInt32(reader["Zangers"]),
                Genre = reader["Genre"].ToString(),
                TeBetalen = Convert.ToDecimal(reader["TeBetalen"]),
                Betaald = reader["Betaald"].ToString(),
                Bevestigd = reader["Bevestigd"].ToString(),
                Kleedkamer = reader["Kleedkamer"].ToString(),
                AcapellaBattle = reader["AcapellaBattle"].ToString(),
                SingAlong = reader["SingAlong"].ToString(),
                Beoordeling = reader["Beoordeling"].ToString(),
                Binnen = Convert.ToInt32(reader["Binnen"]),
                Buiten = Convert.ToInt32(reader["Buiten"]),
                Afgehaakt = reader["Afgehaakt"].ToString()
            },
            new Dictionary<string, object> { { "@festivalId", festivalId } }
            );
    }

    public async Task<List<FestivalParticipationDynamicViewModel>> GetRegistrationPerFestivalAsync(bool filterOutOldGroups = false)
    {
        // Get the first and current festival year
        var yearRange = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetFestivalYearRange,
            reader => new
            {
                Oldest = Convert.ToInt32(reader["Oudste"]),
                Current = Convert.ToInt32(reader["Nieuwste"])
            }
        );

        if (!yearRange.Any())
        {
            return [];
        }

        int startYear = yearRange.First().Oldest;
        int endYear = yearRange.First().Current;

        // Build the query dynamicly
        string query = QueryDefinitions.GetFestivalOverviewQuery(startYear, endYear, filterOutOldGroups);


        List<FestivalParticipationDynamicViewModel> result = await _dataService.ExecuteQueryAsync(
            query,
            reader =>
            {
                FestivalParticipationDynamicViewModel vm = new()
                {
                    ZanggroepId = Convert.ToInt32(reader["zanggroep_id"]),
                    Naam = reader["naam"]?.ToString() ?? "",
                    Stad = reader["stad"]?.ToString() ?? "",
                    Aangemaakt = reader["aangemaakt"]?.ToString() ?? "",
                    DeelnamePerJaar = []
                };

                // Dynamicly add each year
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);

                    if (columnName.StartsWith("Y") &&
                         int.TryParse(columnName.AsSpan(1), out int jaar))
                    {
                        if (!reader.IsDBNull(i))
                        {
                            vm.DeelnamePerJaar[jaar] = reader.GetString(i);
                        }
                    }
                }

                return vm;
            });

        return result;
    }

    public async Task<int> GetCurrentFestivalYearAsync()
    {
        List<int> result = await _dataService.ExecuteQueryAsync(
        QueryDefinitions.GetCurrentFestival,
        reader => Convert.ToInt32(reader["Huidige"])
    );

        return result.FirstOrDefault();
    }

    public async Task UpdatePaymentStatusAsync(uint festivalId, uint groupId, DateTime? paymentDateTime)
    {
        Dictionary<string, object> parameters = new()
        {
            { "@FestivalId", festivalId },
            { "@GroupId", groupId },
            { "@Paid", paymentDateTime ?? (object)DBNull.Value }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync(QueryDefinitions.UpdatePaymentStatus, parameters);
        }
        catch (Exception ex)
        {
            // Write error to db in the future
            Console.WriteLine($"[UpdatePaymentStatusAsync] Error updating payment status: {ex.Message}");
        }
        return;
    }

    public async Task UpdateDropOutStatusAsync(uint festivalId, uint groupId, DateOnly? afgehaaktDate)
    {
        Dictionary<string, object> parameters = new()
        {
            { "@FestivalId", festivalId },
            { "@GroupId", groupId },
            { "@DropOut", afgehaaktDate ?? (object)DBNull.Value }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync(QueryDefinitions.UpdateDropOutStatus, parameters);
        }
        catch (Exception ex)
        {
            // Write error to db in the future
            Console.WriteLine($"[UpdateDropOutStatusAsync] Error updating payment status: {ex.Message}");
        }
        return;

    }

    public async Task UpdateYesNoFieldAsync(uint festivalId, uint groepId, string fieldName, string value)
    {
        var sqlQuery = QueryDefinitions.ModifyChangedGridValue(fieldName);

        await _dataService.ExecuteNonQueryAsync(sqlQuery, new Dictionary<string, object>
        {
            ["@value"] = value,
            ["@FestivalId"] = festivalId,
            ["@GroepId"] = groepId
        });
    }

    public Task<List<AvailableGroupModel>> GetNotRegisteredGroupsAsync(uint festivalId)
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetNotRegisteredGroups,
            reader => new AvailableGroupModel
            {
                ZanggroepId = Convert.ToUInt32(reader["zanggroep_id"]),
                Naam = reader["naam"]?.ToString() ?? string.Empty,
                Standplaats = reader["standplaats"]?.ToString() ?? string.Empty
            },
            new Dictionary<string, object>
            {
                ["@festivalId"] = festivalId
            });
    }

    public Task AddRegistrationAsync(uint festivalId, uint zanggroepId, int aantalDeelnemers, string podiumsoort)
    {
        return _dataService.ExecuteNonQueryAsync(
            QueryDefinitions.AddRegistration,
            new Dictionary<string, object>
            {
                ["@festivalId"] = festivalId,
                ["@zanggroepId"] = zanggroepId,
                ["@aantalDeelnemers"] = aantalDeelnemers,
                ["@podiumsoort"] = podiumsoort,
                ["@ingeschreven"] = DateTime.Now
            });
    }
}
