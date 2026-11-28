using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class RegistrationService(GenericDataService dataService)
{
    private static DateTime? ReadDate(System.Data.Common.DbDataReader reader,string name)=>reader[name] is DBNull?null:Convert.ToDateTime(reader[name]);
    private readonly GenericDataService _dataService = dataService;

    public async Task<List<RegistrationModel>> GetRegistrationsByFestivalIdAsync(uint festivalId)
    {
        var rows=await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetRegistrationsByFestivalId,
            reader => new RegistrationModel
            {
                AvailableFrom = reader["AvailableFrom"].ToString() ?? "",
                AvailableUntil = reader["AvailableUntil"].ToString() ?? "",
                FestivalId = Convert.ToUInt32(reader["festival_id"]),
                Waitlisted = Convert.ToBoolean(reader["Wachtlijst"]),
                GroepId = Convert.ToUInt32(reader["zanggroep_id"]),
                Datum = Convert.ToDateTime(reader["Datum"]),
                Naam = reader["Naam"].ToString(),
                Stad = reader["Stad"].ToString(),
                Podium = reader["Podium"].ToString(),
                Zangers = Convert.ToInt32(reader["Zangers"]),
                Genre = reader["Genre"].ToString(),
                TeBetalen = reader["TeBetalen"] is DBNull ? null : Convert.ToDecimal(reader["TeBetalen"]),
                BetaaldBedrag=reader["BetaaldBedrag"] is DBNull?null:Convert.ToDecimal(reader["BetaaldBedrag"]),
                Betaaldatum=ReadDate(reader,"Betaaldatum"),Bevestigingsdatum=ReadDate(reader,"Bevestigingsdatum"),Afhaakdatum=ReadDate(reader,"Afhaakdatum"),UitersteBetaaldatum=ReadDate(reader,"UitersteBetaaldatum"),
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
        var notes=await RegistrationInvoiceMetadata.LoadAsync(_dataService,festival:festivalId);
        foreach(var row in rows)if(notes.TryGetValue((festivalId,row.GroepId),out var note))note.Apply(row);
        return rows;
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

    public async Task<List<GroupRegistrationModel>> GetGroupRegistrationsByGroupIdAsync(uint groupId)
    {
        var rows=await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetGroupRegistrationsByGroupId,
            reader => new GroupRegistrationModel
            {
                AvailableFrom = reader["AvailableFrom"].ToString() ?? "",
                AvailableUntil = reader["AvailableUntil"].ToString() ?? "",
                FestivalId = Convert.ToUInt32(reader["FestivalId"]),
                Festival = reader["Festival"]?.ToString() ?? string.Empty,
                RegisteredAt = Convert.ToDateTime(reader["RegisteredAt"]),
                Singers = Convert.ToInt32(reader["Singers"]),
                StageType = reader["StageType"]?.ToString() ?? string.Empty,
                TeBetalen=reader["TeBetalen"] is DBNull?null:Convert.ToDecimal(reader["TeBetalen"]),
                BetaaldBedrag=reader["BetaaldBedrag"] is DBNull?null:Convert.ToDecimal(reader["BetaaldBedrag"]),
                Betaaldatum=ReadDate(reader,"Betaaldatum"),Bevestigingsdatum=ReadDate(reader,"Bevestigingsdatum"),Afhaakdatum=ReadDate(reader,"Afhaakdatum"),UitersteBetaaldatum=ReadDate(reader,"UitersteBetaaldatum"),
                Paid = reader["Paid"]?.ToString() ?? string.Empty,
                Confirmed = reader["Confirmed"]?.ToString() ?? string.Empty,
                DroppedOut = reader["DroppedOut"]?.ToString() ?? string.Empty
            },
            new Dictionary<string, object>
            {
                ["@GroupId"] = groupId
            });
        var notes=await RegistrationInvoiceMetadata.LoadAsync(_dataService,group:groupId);
        foreach(var row in rows)if(notes.TryGetValue((row.FestivalId,groupId),out var note))note.Apply(row);
        return rows;
    }

    public async Task<PersonFestivalModel?> GetLatestFestivalForMaintenanceAsync()
    {
        List<PersonFestivalModel> festivals = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetLatestFestivalForPersonMaintenance,
            reader => new PersonFestivalModel
            {
                FestivalId = Convert.ToUInt32(reader["FestivalId"]),
                Festival = reader["Festival"]?.ToString() ?? string.Empty
            });

        return festivals.FirstOrDefault();
    }

    public Task RegisterGroupForCurrentFestivalAsync(uint groupId, uint festivalId)
    {
        return _dataService.ExecuteNonQueryAsync(
            QueryDefinitions.RegisterGroupForCurrentFestival,
            new Dictionary<string, object>
            {
                ["@GroupId"] = groupId,
                ["@FestivalId"] = festivalId
            });
    }
}
