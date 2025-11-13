using System.Net.NetworkInformation;

using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class RegistrationService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<RegistrationModel>> GetRegistrationsByFestivalIdAsync( uint festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetRegistrationsByFestifalId,
            reader => new RegistrationModel
            {
                FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
                GroepId = Convert.ToUInt32( reader [ "zanggroep_id" ] ),
                Datum = Convert.ToDateTime( reader [ "Datum" ] ),
                Naam = reader [ "Naam" ].ToString(),
                Stad = reader [ "Stad" ].ToString(),
                Podium = reader [ "Podium" ].ToString(),
                Zangers = Convert.ToInt32( reader [ "Zangers" ] ),
                Genre = reader [ "Genre" ].ToString(),
                TeBetalen = Convert.ToDecimal( reader [ "TeBetalen" ] ),
                Betaald = reader [ "Betaald" ].ToString(),
                Bevestigd = reader [ "Bevestigd" ].ToString(),
                Kleedkamer = reader [ "Kleedkamer" ].ToString(),
                Binnen = Convert.ToInt32( reader [ "Binnen" ] ),
                Buiten = Convert.ToInt32( reader [ "Buiten" ] ),
                Afgehaakt = reader [ "Afgehaakt" ].ToString()
            },
            new Dictionary<string, object> { { "@festivalId", festivalId } }
            );
    }

    public async Task<List<FestivalParticipationDynamicViewModel>> GetRegistrationdPerFestivalAsync( bool filterOutOldGroups = false )
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

        if ( !yearRange.Any() )
        {
            return [ ];
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
                for ( int jaar = startYear; jaar <= endYear; jaar++ )
                {
                    if ( !reader.IsDBNull( reader.GetOrdinal( jaar.ToString() ) ) )
                    {
                        vm.DeelnamePerJaar [ jaar ] = reader [ jaar.ToString() ].ToString()!;
                    }
                }

                return vm;
            } );

        return result;
    }

    public async Task<int> GetCurrentFestivalYearAsync()
    {
        List<int> result = await _dataService.ExecuteQueryAsync(
        QueryDefinitions.GetCurrentFestival,
        reader => Convert.ToInt32( reader [ "Huidige" ] )
    );

        return result.FirstOrDefault();
    }

    public async Task UpdatePaymentStatusAsync( uint festivalId, uint groupId, DateTime? paymentDateTime )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@FestivalId", festivalId },
            { "@GroupId", groupId },
            { "@Payed", paymentDateTime ?? (object)DBNull.Value }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.UpdatePaymentStatus, parameters );
        }
        catch ( Exception ex)
        {
            // Write error to db in the future
            Console.WriteLine( $"[UpdatePaymentStatusAsync] Error updating payment status: {ex.Message}" );
        }
        return;
    }
}
