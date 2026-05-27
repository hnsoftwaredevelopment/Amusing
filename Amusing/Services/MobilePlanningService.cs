using Amusing.DataReaderExtensions;
using Amusing.Helpers;
using Amusing.Mobile.Shared.Models;
using Amusing.Models;

namespace Amusing.Services;

public class MobilePlanningService(
    PlanningService planningService,
    FestivalService festivalService,
    GenericDataService dataService)
{
    private readonly PlanningService _planningService = planningService;
    private readonly FestivalService _festivalService = festivalService;
    private readonly GenericDataService _dataService = dataService;

    public async Task<MobileFestivalPlanningDto?> GetCurrentPlanningAsync()
    {
        int festivalId = await _festivalService.GetLatestFestivalAsync();
        if (festivalId <= 0)
            return null;

        List<PlanningFestivalsModel> festivals = await _planningService.GetPlanningFestivalsAsync(festivalId);
        PlanningFestivalsModel? festival = festivals.FirstOrDefault();
        if (festival is null)
            return null;

        List<PlanningPerformancesModel> performances = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetMobileCurrentPerformances,
            reader => new PlanningPerformancesModel
            {
                FestivalId = reader.GetMyUInt("FestivalId"),
                GroupId = reader.GetMyUInt("GroupId"),
                GroupName = reader.GetMyString("GroupName"),
                StageId = reader.GetMyUInt("StageId"),
                StageName = reader.GetMyString("StageName"),
                From = reader.GetMyTime("From"),
                To = reader.GetMyTime("To")
            },
            new Dictionary<string, object> { ["@FestivalId"] = festivalId });

        return BuildPlanningDto(festival, festival.FestivalDate, performances, DateTimeOffset.UtcNow);
    }

    public static MobileFestivalPlanningDto BuildPlanningDto(
        PlanningFestivalsModel festival,
        DateOnly festivalDate,
        IEnumerable<PlanningPerformancesModel> performances,
        DateTimeOffset retrievedAt)
    {
        var publicPerformances = performances
            .OrderBy(performance => performance.From)
            .ThenBy(performance => performance.GroupName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(performance => performance.StageName, StringComparer.CurrentCultureIgnoreCase)
            .Select(performance => new MobilePerformanceDto(
                performance.FestivalId,
                performance.GroupId,
                performance.GroupName,
                performance.StageId,
                performance.StageName,
                performance.From,
                performance.To))
            .ToList();

        return new MobileFestivalPlanningDto(
            new MobileFestivalDto(festival.FestivalId, festival.Festival, festivalDate),
            publicPerformances,
            retrievedAt);
    }
}
